using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using FMODUnity;

public class GearController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private const int DragLayer = 1000;
    private const float SpeedThreshold = 0.2f;

    [Header("Gear Physical Parameters")]
    [SerializeField] private GearSettings settings;
    public GearSettings Settings => settings;

    [SerializeField] private bool hasNestedPin = false;
    public bool HasNestedPin => hasNestedPin;

    [SerializeField] private CircleCollider2D tip;

    [Header("SFX")]
    [SerializeField] private EventReference getEventRef;
    [SerializeField] private EventReference dropEventRef;
    [SerializeField] private EventReference slideEventRef;
    [SerializeField] private EventReference installEventRef;
    private Camera sceneCamera;
    public Camera SceneCamera { set => sceneCamera = value; }

    [Header("Scene Parameters")]
    [SerializeField] private int gearsLayer;
    public int Layer
    {
        get => gearsLayer;
        set
        {
            gearsLayer = value;
            GearLayerController.SetLayer(value);
            if (hasNestedPin) NestedPin.Layer = value + 1;
        }
    }

    private int startLayer;
    [SerializeField] private bool draggable = true;
    public bool IsDraggable { get => draggable; set => draggable = value; }
    [SerializeField] private float[] intermediateTargetSpeeds;
    public UnityEvent<int> OnGearReachLevel;
    [HideInInspector] public bool targetGear = false;
    [HideInInspector] public float targetSpeed = 0;

    private float rotationSpeed = 0f;
    public float RotationSpeed
    {
        get => rotationSpeed;
        set
        {
            HandleRotationSpeedChange(value);
            rotationSpeed = value;
        }
    }

    private Vector3 offset;
    private Vector3 startPosition;
    public Vector3 StartPosition { get => startPosition; set => startPosition = value; }

    private PinController basePin = null;
    public PinController BasePin { get => basePin; set => basePin = value; }

    private GearController driveGear = null;
    public GearController DriveGear { get => driveGear; set => driveGear = value; }

    private bool isMoving = false;
    private bool isDragging = false;

    private GearLayerController gearLayerController;
    private GearLayerController GearLayerController
    {
        get
        {
            if (gearLayerController == null)
            {
                gearLayerController = GetComponent<GearLayerController>();
            }
            return gearLayerController;
        }
    }

    private GearOutlineController gearOutlineController;
    private GearOutlineController GearOutlineController
    {
        get
        {
            if (gearOutlineController == null)
            {
                gearOutlineController = GetComponent<GearOutlineController>();
            }
            return gearOutlineController;
        }
    }
   
    private PinController nestedPin;
    public PinController NestedPin
    {
        get
        {
            if (hasNestedPin && nestedPin == null)
            {
                nestedPin = GetComponentInChildren<PinController>(true);
            }
            return nestedPin;
        }
    }

    public void InitializeGear()
    {
        startPosition = transform.position;
        startLayer = gearsLayer;

        tip.radius = settings.tipRadius;
        GetComponent<CircleCollider2D>().radius = settings.gearRadius;
        GearLayerController.SetBaseSprite(settings.baseSprite);
        GearLayerController.SetOutlineSprite(settings.baseSprite);
        GearOutlineController.SetOutlineThickness(settings.outlineThickness, settings.outlineInnerRadius);

        if (hasNestedPin)
        {
            NestedPin.BaseGear = this;
            NestedPin.gameObject.SetActive(true);
        }
        else
        {
            nestedPin = null;
            GetComponentInChildren<PinController>(true).gameObject.SetActive(false);
        }

        Layer = startLayer;
    }

    private void Update()
    {
        if (!isDragging) return;
        Vector3 mousePosition = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, transform.position.z);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!draggable) return;
        ShowOutline();

        Vector3 pointerWorldPosition = sceneCamera.ScreenToWorldPoint(eventData.position);
        offset = transform.position - pointerWorldPosition - Vector3.forward;
        transform.position = pointerWorldPosition + offset;

        GearsSceneController.Instance.ClearConnection(this);
        GearsSceneController.Instance.RecalculateMovement();
        GearsSceneController.Instance.EnableAllPinsOutline(this);
        SoundManager.Instance.Shoot(getEventRef);
        Layer = DragLayer;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!draggable) return;
        Vector3 pointerWorldPosition = sceneCamera.ScreenToWorldPoint(eventData.position);
        transform.position = pointerWorldPosition + offset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!draggable) return;
        GearsSceneController.Instance.DisableAllPinsOutline();
        ShowOutline(false);

        ApplyGear();
    }

    public void ApplyGear(PinController pin = null)
    {
        pin ??= GetPinUnder();
        if (pin == null || !(GearsSceneController.Instance.IsGearFitsBySize(this, pin) && GearsSceneController.Instance.IsGearFitsByType(this, pin)))
        {
            ResetPosition();
            GearsSceneController.Instance.RecalculateMovement();
            return;
        }

        MoveToPin(pin);
    }

    private PinController GetPinUnder()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, settings.pinRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent(out PinController pin) && IsValidPin(pin))
            {
                return pin;
            }
        }
        return null;
    }

    private bool IsValidPin(PinController pin)
    {
        return pin.transform.parent.gameObject != gameObject && !pin.NestedGear && pin.Layer < 100;
    }

    private void MoveToPin(PinController pin)
    {
        GetComponent<SimpleMover>().TargetPoint = pin.transform.position;
        GetComponent<SimpleMover>().RunAnimation(() =>
        {
            basePin = pin;
            basePin.NestedGear = this;
            Layer = pin.Layer;

            transform.SetPositionAndRotation(basePin.transform.position, basePin.transform.rotation);
            rotationSpeed = basePin.BaseGear == null ? basePin.RotationSpeed : basePin.BaseGear.RotationSpeed;
            if (pin.BaseGear != null) InitializePinShaft(this);

            InitializeConnections();

            if (!GearsSceneController.Instance.CanGearsRotateTogether(this))
            {
                ResetPosition();
                GearsSceneController.Instance.RecalculateMovement();
                return;
            }
            if (draggable) SoundManager.Instance.Shoot(installEventRef);

            //GameManager.Instance.StatisticsController.MinigameUpdateMoves();
            GearsSceneController.Instance.AdjustGearAngle(this);
            GearsSceneController.Instance.RecalculateMovement();
        });
    }

    private void InitializePinShaft(GearController gear)
    {
        if (gear?.BasePin?.BaseGear == null) return;
        GearsSceneController.Instance.AddConnection(this, basePin.BaseGear);
    }

    private void InitializeConnections()
    {
        Collider2D[] tipColliders = Physics2D.OverlapCircleAll(transform.position, settings.tipRadius * transform.localScale.x);
        foreach (Collider2D collider in tipColliders)
        {
            GearController gear = collider.GetComponentInParent<GearController>();
            if (gear != null && gear.gameObject != gameObject && gear.Layer == gearsLayer)
            {
                GearsSceneController.Instance.AddConnection(this, gear);
            }
        }
    }

    public void ResetPosition()
    {
        GearsSceneController.Instance.ClearConnection(this);
        SoundManager.Instance.Shoot(dropEventRef);
        SoundManager.Instance.Shoot(slideEventRef);
        Layer = startLayer;
        GetComponent<SimpleMover>().SetStartAsTarget();
        GetComponent<SimpleMover>().RunAnimation();
    }

    public void ShowOutline(bool show = true)
    {
        GearOutlineController.ShowOutline(show);
    }

    private void HandleRotationSpeedChange(float newSpeed)
    {
        if (rotationSpeed == 0 && newSpeed != 0)
        {
            GearsSceneController.Instance.GearPlaced(true);
        }
        if (rotationSpeed != 0 && newSpeed == 0)
        {
            GearsSceneController.Instance.GearPlaced(false);
        }

        for (int i = 0; i < intermediateTargetSpeeds.Length; i++)
        {
            if (Mathf.Abs(Mathf.Abs(newSpeed) - intermediateTargetSpeeds[i]) <= SpeedThreshold)
            {
                OnGearReachLevel?.Invoke(i);
                break;
            }
        }

        if (!targetGear) return;

        if (targetSpeed == 0)
        {
            UpdateMovingState(newSpeed != 0);
            return;
        }

        bool isSpeedWithinThreshold = Mathf.Abs(Mathf.Abs(newSpeed) - targetSpeed) <= SpeedThreshold;
        UpdateMovingState(isSpeedWithinThreshold);
    }

    private void UpdateMovingState(bool isMoving)
    {
        if (this.isMoving == isMoving) return;

        this.isMoving = isMoving;
        GearsSceneController.Instance.CountTargetGears(isMoving);
    }
}
