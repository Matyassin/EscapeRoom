using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FpsController : MonoBehaviour
{
    [Header("Controller Permissions")]
    public bool CanMove = true;
    public bool CanLook = true;
    public bool CanSprint = true;
    public bool CanJump = true;
    public bool CanCrouch = true;
    public bool CanLean = true;
    public bool CanHeadbob = true;

    [Header("Movement")]
    [SerializeField, Range(1f, 4f)] public float m_MoveSpeed = 2f;
    [SerializeField, Range(2f, 80f)] private float m_SprintSpeed = 4f;
    [SerializeField, Range(1, 10)] private float m_TimeToChangeMoveVelocity = 4f;
    public CharacterController Controller { get; private set; }
    public Vector3 PlayerVelocity { get; private set; }

    private float m_MoveCurrentSpeed;
    private bool m_IsSprinting = false;
    private RaycastHit m_SlopeHit;

    [Header("Look")]
    [SerializeField, Range(0.1f, 1f)] private float m_MouseSens = 1f;
    private Camera m_MainCamera;
    private Transform m_PlayerHead;
    private float m_TargetPitch;

    [Header("Jumping")]
    [SerializeField, Range(1f, 10f)] private float m_JumpForce = 6f;
    private Vector3 m_JumpDirection = Vector3.zero;
    private float m_VerticalVelocity = 0f;
    private float m_MoveSpeedWhileJumping;
    private bool m_IsInAir = false;

    [Header("Landing")]
    [SerializeField, Range(0.1f, 0.5f)] private float m_LandDuckAmount = 0.2f;
    [SerializeField, Range(0.01f, 0.1f)] private float m_LandDuckSmoothTime = 0.08f;
    private Coroutine m_LandCoroutine = null;
    private float m_LandDuckOffset = 0f;

    [Header("Crouching")]
    [SerializeField, Range(0.5f, 1.2f)] private float m_CrouchSpeed = 1f;
    [SerializeField, Range(0.5f, 2f)] private float CrouchHeight = 1.25f;
    [SerializeField, Range(1f, 2f)] private float StandHeight = 2f;
    [SerializeField, Range(1f, 30f)] private float m_TimeToCrouch = 10f;
    public bool IsCrouched { get; private set; } = false;
    public float CrouchOffsetY { get; private set; } = 0f;
    private float m_CrouchCurrentHeight;
    private float m_CrouchTargetHeight;
    private bool m_IsCrouchingTransition = false;

    [Header("Headbob")]
    [Tooltip("How frequently the headbob animation plays.")]
    [SerializeField, Range(0.5f, 5f)] private float m_HeadbobFrequency = 3f;
    [Tooltip("How big is the movement of the headbob.")]
    [SerializeField, Range(0.01f, 0.1f)] private float m_HeadbobAmplitude = 0.02f;
    [Tooltip("The speed of the headbob animation.")]
    [SerializeField, Range(0.1f, 5f)] private float m_HeadbobSpeedMultiplier = 1;
    [Tooltip("The time it takes in seconds for the headbob to start following the head.")]
    [SerializeField, Range(1f, 10f)] private float m_HeadbobLerpTime = 2.5f;
    public Vector3 HeadbobOffsetY { get; private set; } = Vector3.zero;
    private Vector3 m_InitialHeadPosition;
    private float m_CurrentHeadbobAmplitude = 0f;
    private float m_HeadbobTimer = 0f;

    [Header("Leaning")]
    [SerializeField, Range(10f, 30f)] private float m_LeanAngle = 20f;
    [SerializeField, Range(0.1f, 1f)] private float m_LeanOffsetX = 0.5f;
    [SerializeField, Range(1f, 20f)] private float m_LeanSpeed = 10f;
    private Vector3 m_LeanCurrentPositionOffset = Vector3.zero;
    private Vector3 m_LeanTargetPositionOffset = Vector3.zero;
    public float LeanCurrentRotation { get; private set; } = 0f;
    private float m_LeanTargetRotation = 0f;

    [Header("Sliding")]
    [SerializeField, Range(0.5f, 5f)] private float m_SlideSpeed = 1.5f;
    private bool m_IsSliding = false;

    [Header("Feet Transform")]
    [SerializeField] private Transform m_Feet;

    private float m_PreviousBobY = 0;
    private bool m_HasPlayedFootstepLastFrame = false;
    private bool m_WasGroundedLastFrame = false;

    private void Awake()
    {
        Controller = GetComponent<CharacterController>();
        m_MainCamera = Camera.main;
        m_PlayerHead = transform.GetChild(0);

        m_MoveCurrentSpeed = m_MoveSpeed;
        m_InitialHeadPosition = m_PlayerHead.transform.localPosition;

        m_CrouchCurrentHeight = Controller.height;
        m_CrouchTargetHeight = Controller.height;
    }

    private void Start()
    {
        CursorUI.Instance.ToggleCursor(false);

        GameplayInput.Instance.OnJumpStarted += JumpPressed;
        GameplayInput.Instance.OnCrouchStarted += CrouchPressed;

        GameplayInput.Instance.OnLeanStarted += OnLeanStart;
        GameplayInput.Instance.OnLeanCanceled += OnLeanEnd;
    }

    private void OnDestroy()
    {
        GameplayInput.Instance.OnJumpStarted -= JumpPressed;
        GameplayInput.Instance.OnCrouchStarted -= CrouchPressed;

        GameplayInput.Instance.OnLeanStarted -= OnLeanStart;
        GameplayInput.Instance.OnLeanCanceled -= OnLeanEnd;
    }

    private void LateUpdate()
    {
        HandleHeadbob();
        HandleHeadPosition();
    }

    private void Update()
    {
        HandleLook();
        HandleLean();
        HandleMovement();
        HandleCrouch();
        HandleJump();
    }

    public void ToggleController(bool value)
    {
        CanMove = value;
        CanLook = value;
        CanSprint = value;
        CanJump = value;
        CanCrouch = value;
        CanLean = value;
        CanHeadbob = value;
    }

    private void HandleMovement()
    {
        Vector3 inputDirection = Vector3.zero;

        if (!m_IsInAir && CanMove)
        {
            inputDirection = GameplayInput.Instance.Move;
            inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);
        }

        Vector3 moveDirection;

        if (m_IsInAir)
        {
            moveDirection = m_JumpDirection;
            m_IsSliding = false;
        }
        else if (IsOnSteepSlope(out Vector3 slideDir))
        {
            moveDirection = slideDir * m_SlideSpeed;
            m_IsSliding = true;
        }
        else if (IsOnSlope())
        {
            Vector3 flatMovement = transform.forward * inputDirection.z + transform.right * inputDirection.x;
            moveDirection = Vector3.ProjectOnPlane(flatMovement, m_SlopeHit.normal).normalized * flatMovement.magnitude;
            m_IsSliding = false;
        }
        else
        {
            moveDirection = transform.forward * inputDirection.z + transform.right * inputDirection.x;
            m_IsSliding = false;
        }

        bool sprintInput = (GameplayInput.Instance.Move.z > 0.5f) && GameplayInput.Instance.Sprint;
        m_IsSprinting = Controller.isGrounded && sprintInput && !IsCrouched && CanSprint;
        float targetSpeed = IsCrouched ? m_CrouchSpeed : m_IsSprinting ? m_SprintSpeed : m_MoveSpeed;

        if (m_IsInAir)
            m_MoveCurrentSpeed = m_MoveSpeedWhileJumping;
        else
            m_MoveCurrentSpeed = Mathf.Lerp(m_MoveCurrentSpeed, targetSpeed, Time.deltaTime * m_TimeToChangeMoveVelocity);

        Vector3 velocity = moveDirection * m_MoveCurrentSpeed;
        PlayerVelocity = velocity;
        velocity.y = m_VerticalVelocity;

        Controller.Move(velocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        if (!CanLook)
            return;

        Vector2 lookInput = GameplayInput.Instance.Look;

        //float lookSensitivity = 1f; //SettingsManager.Instance.ControlsSettings.LookSensitivity;
        int mouseInvert = 1; //SettingsManager.Instance.ControlsSettings.InvertLookY ? -1 : 1;

        float lookX = lookInput.x * m_MouseSens;
        float lookY = lookInput.y * m_MouseSens * mouseInvert;

        // Apply vertical (pitch)
        m_TargetPitch -= lookY;
        m_TargetPitch = Mathf.Clamp(m_TargetPitch, -80f, 80f);
        m_PlayerHead.localRotation = Quaternion.Euler(m_TargetPitch, 0f, 0f);

        // Apply horizontal (yaw)
        transform.Rotate(Vector3.up * lookX);
    }

    private void JumpPressed()
    {
        if (!CanJump || !Controller.isGrounded || IsCrouched)
            return;

        m_VerticalVelocity = m_JumpForce;

        Vector3 inputDir = GameplayInput.Instance.Move;
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);
        m_JumpDirection = transform.forward * inputDir.z + transform.right * inputDir.x;
        m_JumpDirection.y = 0f;
        m_JumpDirection.Normalize();

        float jumpBaseSpeed = IsCrouched ? m_CrouchSpeed : m_IsSprinting ? m_SprintSpeed : m_MoveSpeed;

        m_MoveSpeedWhileJumping = jumpBaseSpeed;
        m_IsInAir = true;
    }

    private void HandleJump()
    {
        if (Controller.isGrounded && m_VerticalVelocity < 0f)
            m_VerticalVelocity = -2f;
        else
            m_VerticalVelocity += Physics.gravity.y * Time.deltaTime;

        if ((Controller.collisionFlags & CollisionFlags.Above) != 0 && m_VerticalVelocity > 0f)
            m_VerticalVelocity = 0f;

        if (!m_WasGroundedLastFrame && Controller.isGrounded && m_IsInAir)
        {
            //PlayJumpAudio(m_JumpLandLibraries); // The issue with this is the slope movement, if we can't fix that we don't play ground hitting sounds.
            m_IsInAir = false;

            if (m_LandCoroutine != null)
                StopCoroutine(m_LandCoroutine);

            m_LandCoroutine = StartCoroutine(LandDuck());
        }

        m_WasGroundedLastFrame = Controller.isGrounded;
    }

    private void CrouchPressed()
    {
        if (!CanCrouch)
            return;

        bool canStandUp = !Physics.CheckSphere(Controller.transform.position + Vector3.up * StandHeight, 0.5f, ~LayerMask.GetMask("Player") & ~LayerMask.GetMask("Ignore Raycast"));

        if (Controller.isGrounded && !m_IsCrouchingTransition)
        {
            if (!IsCrouched || (IsCrouched && canStandUp))
            {
                IsCrouched = !IsCrouched;
                m_CrouchTargetHeight = IsCrouched ? CrouchHeight : StandHeight;
                m_IsCrouchingTransition = true;
            }
        }
    }

    private void HandleCrouch()
    {
        m_CrouchCurrentHeight = Mathf.Lerp(m_CrouchCurrentHeight, m_CrouchTargetHeight, Time.deltaTime * m_TimeToCrouch);
        Controller.height = m_CrouchCurrentHeight;
        Controller.center = new Vector3(0f, m_CrouchCurrentHeight / 2f, 0f);

        if (Mathf.Abs(m_CrouchCurrentHeight - m_CrouchTargetHeight) < 0.01f)
        {
            m_CrouchCurrentHeight = m_CrouchTargetHeight;
            m_IsCrouchingTransition = false;
        }

        float standingY = 1.8f;
        float crouchedY = 1.0f;

        float targetY = IsCrouched ? crouchedY : standingY;
        float targetOffset = targetY - standingY;

        CrouchOffsetY = Mathf.Lerp(CrouchOffsetY, targetOffset, Time.deltaTime * m_TimeToCrouch);
    }

    public void HandleCrouchInstantly(bool isCrouched)
    {
        IsCrouched = isCrouched;
        m_CrouchTargetHeight = isCrouched ? CrouchHeight : StandHeight;
        m_CrouchCurrentHeight = m_CrouchTargetHeight;

        Controller.height = m_CrouchTargetHeight;
        Controller.center = new Vector3(0f, m_CrouchTargetHeight / 2f, 0f);
        CrouchOffsetY = IsCrouched ? -0.8f : 0.0f;

        m_IsCrouchingTransition = false;
    }

    private void OnLeanStart()
    {
        if (InputHandler.Instance.IsCurrentControlGamepad())
            CanMove = false;
    }

    private void OnLeanEnd()
    {
        CanMove = true;
    }

    private void HandleLean()
    {
        if (!CanLean)
            return;

        m_LeanTargetRotation = -GameplayInput.Instance.Lean * m_LeanAngle; // Negative to match Q=left, E=right
        m_LeanTargetPositionOffset = new Vector3(GameplayInput.Instance.Lean * m_LeanOffsetX, 0f, 0f);

        // Prevent leaning into walls
        if (m_LeanTargetPositionOffset != Vector3.zero)
        {
            Vector3 direction = m_MainCamera.transform.right * Mathf.Sign(m_LeanTargetPositionOffset.x);
            Vector3 origin = transform.position + Vector3.up * m_CrouchCurrentHeight - direction * 0.05f;
            float radius = 0.5f;
            float desiredOffset = Mathf.Abs(m_LeanTargetPositionOffset.x);
            float maxDistance = desiredOffset + 0.035f;

            if (Physics.SphereCast(origin, radius, direction, out RaycastHit hit, maxDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                float leanFraction = Mathf.Clamp01((hit.distance - 0.05f) / desiredOffset);

                m_LeanTargetPositionOffset *= leanFraction;
                m_LeanTargetRotation *= leanFraction;
            }
        }

        // Position lerp
        m_LeanCurrentPositionOffset = Vector3.Lerp(
            m_LeanCurrentPositionOffset,
            m_LeanTargetPositionOffset,
            Time.deltaTime * m_LeanSpeed
        );

        // Rotation lerp
        LeanCurrentRotation = Mathf.Lerp(
            LeanCurrentRotation,
            m_LeanTargetRotation,
            Time.deltaTime * m_LeanSpeed
        );

        m_PlayerHead.transform.localRotation = Quaternion.Euler(
            m_PlayerHead.localEulerAngles.x,
            m_PlayerHead.localEulerAngles.y,
            LeanCurrentRotation
        );
    }

    private void HandleHeadbob()
    {
        if (!CanHeadbob)
            return;

        bool isMoving = Controller.velocity.magnitude > 0.1f && Controller.isGrounded;

        float targetAmplitude = isMoving ? m_HeadbobAmplitude : 0f;
        m_CurrentHeadbobAmplitude = Mathf.Lerp(m_CurrentHeadbobAmplitude, targetAmplitude, Time.deltaTime * m_HeadbobLerpTime);

        if ((isMoving || m_CurrentHeadbobAmplitude > 0.01f) && !m_IsSliding)
        {
            m_HeadbobTimer += Time.deltaTime * m_HeadbobFrequency * Controller.velocity.magnitude * m_HeadbobSpeedMultiplier;

            float bobX = Mathf.Sin(m_HeadbobTimer) * m_CurrentHeadbobAmplitude;
            float bobY = Mathf.Cos(m_HeadbobTimer * 2f) * m_CurrentHeadbobAmplitude;

            HeadbobOffsetY = new Vector3(bobX, bobY, 0f);

            if (Controller.isGrounded && Controller.velocity.magnitude > 0.1f)
            {
                // If descending and at the bottom of the bob, play footstep once
                if (!m_HasPlayedFootstepLastFrame &&
                    m_PreviousBobY > bobY &&
                    Mathf.Abs(bobY - m_CurrentHeadbobAmplitude * -1f) < 0.01f &&
                    Controller.velocity.magnitude > 0.25f)
                {
                    if (Physics.Raycast(m_Feet.position, Vector3.down, 0.5f, LayerMask.GetMask("Stone")))
                        AudioManager.Instance.PlayRandom(AudioCategory.FootstepStone, m_Feet.position);
                    else
                        AudioManager.Instance.PlayRandom(AudioCategory.FootstepWood, m_Feet.position);
                    
                    m_HasPlayedFootstepLastFrame = true;
                }

                if (bobY > m_PreviousBobY)
                {
                    m_HasPlayedFootstepLastFrame = false;
                }
            }

            m_PreviousBobY = bobY;
        }
        else
        {
            m_HeadbobTimer = 0f;
        }
    }

    private void HandleHeadPosition()
    {
        Vector3 finalOffset = m_InitialHeadPosition
                            + Vector3.up * CrouchOffsetY
                            + m_LeanCurrentPositionOffset
                            + HeadbobOffsetY
                            + Vector3.up * m_LandDuckOffset;

        m_PlayerHead.localPosition = finalOffset;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb != null && !rb.isKinematic && rb.CompareTag("LightInteractable"))
        {
            Vector3 pushDir = Vector3.ProjectOnPlane(hit.moveDirection, Vector3.up).normalized;

            float momentum = Mathf.Clamp(Controller.velocity.magnitude, 0.5f, 1f);
            float forceAmount = momentum / rb.mass;

            rb.AddForce(pushDir * forceAmount, ForceMode.VelocityChange);
        }
    }

    private bool IsOnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit m_SlopeHit, Controller.height / 2 + 0.5f))
        {
            if (m_SlopeHit.normal != Vector3.up)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsOnSteepSlope(out Vector3 slideDir)
    {
        if (Physics.SphereCast(transform.position + Vector3.up, Controller.radius, Vector3.down, out RaycastHit hit, Controller.height / 2f, LayerMask.GetMask("Ground")))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle >= Controller.slopeLimit)
            {
                slideDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
                return true;
            }
        }

        slideDir = Vector3.zero;
        return false;
    }

    private IEnumerator LandDuck()
    {
        float target = -m_LandDuckAmount;
        float downVelocity = 0f;

        // Duck down
        while (Mathf.Abs(m_LandDuckOffset - target) > 0.01f)
        {
            m_LandDuckOffset = Mathf.SmoothDamp(m_LandDuckOffset, target, ref downVelocity, m_LandDuckSmoothTime * 0.75f);
            yield return null;
        }

        // Return up
        float upVelocity = 0f;
        while (Mathf.Abs(m_LandDuckOffset) > 0.01f)
        {
            m_LandDuckOffset = Mathf.SmoothDamp(m_LandDuckOffset, 0f, ref upVelocity, m_LandDuckSmoothTime);
            yield return null;
        }

        m_LandDuckOffset = 0f;
        m_LandCoroutine = null;
    }
}
