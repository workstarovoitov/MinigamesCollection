using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Architecture;

public enum InputDeviceType
{
    Keyboard,
    Gamepad,
    Touchscreen,
    Unknown
}

[System.Serializable]
public class KeyBindingPair
{
    public string actionName;
    public InputActionReference inputAction;
    public string bindingPath;
}

public class InputController : MonoBehaviour, IService
{
    private InputActions actions;
    public InputActions Actions { get => actions; }
    [SerializeField] private InputDeviceType curentInputDevice;
    public InputDeviceType CurrentInputDevice { get => curentInputDevice; set => curentInputDevice = value; }

    public void Initialize()
    {
        // Initialization logic here
    }

    private void OnEnable()
    {
        actions.Enable();
    }

    private void OnDisable()
    {
        actions.Disable();
    }

    private void Awake()
    {
        ServiceLocator.Register(this);
        actions = new InputActions();
    }

    private void Update()
    {
        if (curentInputDevice != InputDeviceType.Gamepad && Gamepad.current != null)
        {
            if (Gamepad.current.allControls.Any(control => control is ButtonControl button && button.wasPressedThisFrame))
            {
                curentInputDevice = InputDeviceType.Gamepad;
                return;
            }
        }

        if (curentInputDevice != InputDeviceType.Touchscreen && Touchscreen.current != null)
        {
            if (Touchscreen.current.touches.Any(touch => touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began))
            {
                curentInputDevice = InputDeviceType.Touchscreen;
                return;
            }
        }

        if (curentInputDevice != InputDeviceType.Keyboard && Keyboard.current != null)
        {
            if (Keyboard.current.allControls.Any(control => control is KeyControl key && key.wasPressedThisFrame))
            {
                curentInputDevice = InputDeviceType.Keyboard;
                return;
            }
        }

        if (curentInputDevice != InputDeviceType.Keyboard && Mouse.current != null)
        {
            if (Mouse.current.allControls.Any(control => control is ButtonControl button && button.wasPressedThisFrame))
            {
                curentInputDevice = InputDeviceType.Keyboard;
                return;
            }
        }
    }

    public string KeyForAction(InputAction action)
    {
        return InputControlPath.ToHumanReadableString(
            action.bindings[0].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
    }
}
