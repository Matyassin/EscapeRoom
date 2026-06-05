using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class GameplayInput : MonoBehaviour
{
    public Vector3 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public Vector2 ItemRotate { get; private set; }
    public bool Sprint { get; private set; }
    public bool Interact { get; private set; }
    public float Lean { get; private set; }
    public float Scroll { get; private set; }

    public event Action OnCrouchStarted;
    public event Action OnJumpStarted;
    public event Action OnInteractStarted;
    public event Action OnInteractCanceled;
    public event Action OnThrowStarted;
    public event Action OnItemRotateStarted;
    public event Action OnItemRotateCanceled;
    public event Action OnLeanStarted;
    public event Action OnLeanCanceled;
    public event Action OnPauseStarted;

    private PlayerInput m_Input;

    public static GameplayInput Instance { get; private set; }

    private void Awake()
    {
        m_Input = GetComponent<PlayerInput>();

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Move
        m_Input.actions["Move"].performed += (ctx) =>
        {
            Vector2 move = ctx.ReadValue<Vector2>();
            Move = new Vector3(move.x, 0.0f, move.y);
        };

        m_Input.actions["Move"].canceled += (ctx) => Move = Vector3.zero;

        // Look
        m_Input.actions["Look"].performed += (ctx) => Look = ctx.ReadValue<Vector2>();
        m_Input.actions["Look"].canceled += (ctx) => Look = Vector2.zero;

        // Crouch
        m_Input.actions["Crouch"].started += (ctx) => OnCrouchStarted?.Invoke();

        // Jump
        m_Input.actions["Jump"].started += (ctx) => OnJumpStarted?.Invoke();

        // Sprint
        m_Input.actions["Sprint"].started += (ctx) => Sprint = true;
        m_Input.actions["Sprint"].canceled += (ctx) => Sprint = false;

        // Interact
        m_Input.actions["Interact"].started += (ctx) =>
        {
            Interact = true;
            OnInteractStarted?.Invoke();
        };

        m_Input.actions["Interact"].canceled += (ctx) =>
        {
            Interact = false;
            OnInteractCanceled?.Invoke();
        };

        // Throw
        m_Input.actions["Throw"].started += (ctx) => OnThrowStarted?.Invoke();

        // Item Rotate
        m_Input.actions["ItemRotate"].started += (ctx) => OnItemRotateStarted?.Invoke();
        m_Input.actions["ItemRotate"].performed += (ctx) => ItemRotate = ctx.ReadValue<Vector2>();
        m_Input.actions["ItemRotate"].canceled += (ctx) =>
        {
            ItemRotate = Vector2.zero;
            OnItemRotateCanceled?.Invoke();
        };

        // Lean
        m_Input.actions["Lean"].started += (ctx) => OnLeanStarted?.Invoke();
        m_Input.actions["Lean"].performed += (ctx) => Lean = ctx.ReadValue<Vector2>().x;
        m_Input.actions["Lean"].canceled += (ctx) =>
        {
            Lean = 0.0f;
            OnLeanCanceled?.Invoke();
        };

        // Pause
        m_Input.actions["Pause"].started += (ctx) => OnPauseStarted?.Invoke();

        // Mouse Scroll
        m_Input.actions["MouseScroll"].performed += (ctx) =>
        {
            Scroll = ctx.ReadValue<Vector2>().y;
        };

        m_Input.actions["MouseScroll"].canceled += (ctx) =>
        {
            Scroll = 0.0f;
        };
    }
}
