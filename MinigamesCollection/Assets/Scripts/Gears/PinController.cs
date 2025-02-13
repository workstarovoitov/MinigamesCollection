using UnityEngine;

public class PinController : MonoBehaviour
{
    [Header("Level settings")]
    [SerializeField] private float rotationSpeed = 0f;
    public float RotationSpeed { get => rotationSpeed; }

    [SerializeField] private int gearsLayer = 1;
    public int Layer
    {
        get => gearsLayer;
        set
        {
            gearsLayer = value;
            GearLayerController.SetLayer(value);
        }
    }

    [SerializeField] private bool isBase = false;
    public bool IsBase
    {
        get => isBase;
        set
        {
            isBase = value;
            UpdateSpriteBasedOnType();
        }
    }

    [Header("Pin settings")]
    [SerializeField] private Sprite baseSprite;
    [SerializeField] private Sprite nestedSprite;

    [ColorUsage(true, true)][SerializeField] private Color baseColorAllow;
    [ColorUsage(true, true)][SerializeField] private Color colorAllow;
    [ColorUsage(true, true)][SerializeField] private Color baseColorDeny;
    [ColorUsage(true, true)][SerializeField] private Color colorDeny;

    private GearController baseGear;
    public GearController BaseGear { get; set; }

    private GearController nestedGear = null;
    public GearController NestedGear { get; set; }

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

    public void InitializePin()
    {
        Layer = gearsLayer;
        UpdateSpriteBasedOnType();
        SetAsPin();
    }

    public void ShowOutline(bool show = true, bool allow = true, bool shootSound = true)
    {
        var baseColor = allow ? baseColorAllow : baseColorDeny;
        var color = allow ? colorAllow : colorDeny;
        if (show) GearOutlineController.SetOutlineColor(baseColor, color);
        GearOutlineController.ShowOutline(show, shootSound);
    }

    private void UpdateSpriteBasedOnType()
    {
        var sprite = isBase ? baseSprite : nestedSprite;
        GearLayerController.SetBaseSprite(sprite);
    }

    private void SetAsPin()
    {
        GearLayerController.IsGear = false;
    }
}
