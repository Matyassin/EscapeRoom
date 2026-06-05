using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    public ControlType CurrentControl { get; private set; } = ControlType.Keyboard;
    public event Action<ControlType> OnControlsChanged;

    public PlayerInput Handle { get; private set; }

    public static InputHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Handle = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        Handle.onControlsChanged += OnControlsChangedInput;
    }

    public void SwitchActionMap(ActionMapType actionMap)
    {
        switch (actionMap)
        {
            case ActionMapType.Gameplay:
                Handle.SwitchCurrentActionMap("Gameplay");
                break;
            case ActionMapType.UI:
                Handle.SwitchCurrentActionMap("UI");
                break;
        }
    }

    public bool IsCurrentControlGamepad()
    {
        return CurrentControl == ControlType.Xbox ||
               CurrentControl == ControlType.Playstation;
    }

    public bool IsCurrentControlKeyboard()
    {
        return CurrentControl == ControlType.Keyboard;
    }

    private void OnControlsChangedInput(PlayerInput playerInput)
    {
        ControlType controlType = ControlType.None;

        switch (playerInput.currentControlScheme)
        {
            case "Keyboard&Mouse":
                controlType = ControlType.Keyboard;
                break;
            case "XboxController":
                controlType = ControlType.Xbox;
                break;
            case "PlaystationController":
                controlType = ControlType.Playstation;
                break;
            default:
                break;
        }

        CurrentControl = controlType;
        OnControlsChanged?.Invoke(controlType);
    }

    public enum ControlType
    {
        None,
        Keyboard,
        Xbox,
        Playstation
    }

    public enum ActionMapType
    {
        Gameplay,
        UI
    }
}
