using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using FMODUnity;
using UnityEngine.InputSystem;
using Architecture;

public class BasePopupController : MonoBehaviour
{
    [SerializeField] internal EventReference openSFX;
    [SerializeField] internal EventReference closeSFX;

    [SerializeField] internal Button cancelButton;
    [SerializeField] internal float lifetime = -1f;

    [SerializeField] private bool showOnStart = false;
    [SerializeField] private bool pausesGame = true;
    [SerializeField] private bool blocksPrevious = true;
    public bool PausesGame { get => pausesGame; }
    public bool BlocksPrevious { get => blocksPrevious; }
    [SerializeField] private bool skipOnAnyKey = false;
    [SerializeField] private bool skipOnSpace = false;

    [SerializeField] internal UnityEvent onOpen;
    [SerializeField] internal UnityEvent onClose;

    private InputAction actionCancel = new();
    private InputAction actionAnykey = new();
    private InputAction actionSubmit = new();

    private UIAlphaFader fader;

    protected virtual void OnEnable()
    {
        fader = GetComponent<UIAlphaFader>();

        var inputController = ServiceLocator.Get<InputController>();
        if (inputController != null)
        {
            actionCancel = inputController.Actions.UI.Cancel;
            if (cancelButton != null)
            {
                actionCancel.performed += BackPerformed;
                actionCancel.Enable();
            }
            else
            {
                actionCancel.performed += HideWindow;
            }

            if (skipOnAnyKey)
            {
                actionAnykey = inputController.Actions.UI.Anykey;
                actionAnykey.performed += HideWindow;
                actionAnykey.Enable();
            }
            if (skipOnSpace)
            {
                actionSubmit = inputController.Actions.UI.Submit;
                actionSubmit.performed += HideWindow;
                actionSubmit.Enable();
            }
        }
    }

    protected virtual void Start()
    {
        fader = GetComponent<UIAlphaFader>();
        if (showOnStart) ShowWindow();
    }

    protected virtual void OnDisable()
    {
        if (actionCancel != null) { actionCancel.performed -= BackPerformed; }
        if (actionAnykey != null) { actionAnykey.performed -= HideWindow; }
        if (actionSubmit != null) { actionSubmit.performed -= HideWindow; }
    }

    public void HideWindow(InputAction.CallbackContext ctx)
    {
        HideWindow();
    }

    public virtual void HideWindow()
    {
        CancelInvoke(nameof(HideWindow));
        ServiceLocator.Get<SoundManager>()?.Shoot(closeSFX);
        foreach (Button button in GetComponentsInChildren<Button>())
        {
            button.interactable = false;
        }

        if (fader)
        {
            fader.RunAnimationFadeOut(() =>
            {
                ServiceLocator.Get<PopupsManager>()?.RemovePopup(this);

                onClose?.Invoke();
                gameObject.SetActive(false);

            });
            return;
        }

        ServiceLocator.Get<PopupsManager>()?.RemovePopup(this);

        onClose?.Invoke();
        gameObject.SetActive(false);
    }

    public virtual void HideWindowSilent()
    {
        CancelInvoke(nameof(HideWindow));
        ServiceLocator.Get<PopupsManager>()?.RemovePopup(this);
        gameObject.SetActive(false);
    }

    public virtual void ShowWindow()
    {
        var popupController = ServiceLocator.Get<PopupsManager>();
        if (popupController != null && popupController.IsOpened(this)) return;

        popupController?.AddPopup(this);
        gameObject.SetActive(true);
        if (fader) fader.RunAnimationFadeIn();
        ServiceLocator.Get<SoundManager>()?.Shoot(openSFX);
        onOpen?.Invoke();
        foreach (Button button in GetComponentsInChildren<Button>())
        {
            button.interactable = true;
        }

        if (lifetime > 0f) Invoke(nameof(HideWindow), lifetime);
    }

    public virtual void BackPerformed(InputAction.CallbackContext ctx)
    {
        if (cancelButton != null) cancelButton.onClick.Invoke();
    }

    public void FireEvent(GameEvent gameEvent)
    {
        gameEvent?.Invoke();
    }

    public void ShootSFX(string sfxPath)
    {
        ServiceLocator.Get<SoundManager>()?.Shoot(sfxPath);
    }
}
