using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(FpsController))]
[RequireComponent(typeof(PlayerCasting))]
[RequireComponent(typeof(CharacterController))]
public class DragRigidbody : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform m_RaycastOrigin;
    [SerializeField] private Transform m_PlayerFeet;
    [SerializeField] private Transform m_Guide;

    [Header("Drag Settings")]
    [SerializeField, Range(0.5f, 20f)] private float m_DragSpeed = 1.0f;
    [SerializeField, Range(100f, 1000f)] private float m_ThrowForce = 500f;
    [SerializeField, Range(3f, 4f)] private float m_MaxDistance = 3.0f;
    [SerializeField, Range(0.0f, 2.0f)] private float m_MinDistanceLigth;
    [SerializeField, Range(0.0f, 4.5f)] private float m_MinDistanceHeavy;
    private float m_MinDistance = 0f;

    [Header("Rotation Settings")]
    [SerializeField, Range(5f, 200f)] private float m_RotationSpring = 75f;
    [SerializeField, Range(1f, 4f)] private float m_ManualRotationSpeed = 2f;
    [SerializeField, Range(1f, 5f)] private float m_RotationSpeedClamp = 2.5f;

    [Header("Tags")]
    [SerializeField] private string m_DragTagLight;
    [SerializeField] private string m_DragTagHeavy;

    public event Action OnDragStart;
    public event Action OnDragEnd;

    private Quaternion m_HeldObjectRotationOffset = Quaternion.identity;
    private Transform m_HeldObject = null;
    private Rigidbody m_HeldRb = null;
    private Collider m_Collider;
    private CharacterController m_CharacterController;

    private readonly float m_PickupCooldown = 1f;

    private bool m_IsHolding = false;
    private bool m_IsRotating = false;
    private bool m_Thrown = false;

    public bool IsDragging => m_IsHolding;
    public bool IsRotating => m_IsRotating;

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        GameplayInput.Instance.OnInteractStarted += TryStartDragging;
        GameplayInput.Instance.OnThrowStarted += ThrowHeldObject;

        GameplayInput.Instance.OnItemRotateStarted += OnRotateStart;
        GameplayInput.Instance.OnItemRotateCanceled += OnRotateEnd;
    }

    private void OnDestroy()
    {
        GameplayInput.Instance.OnInteractStarted -= TryStartDragging;
        GameplayInput.Instance.OnThrowStarted -= ThrowHeldObject;

        GameplayInput.Instance.OnItemRotateStarted -= OnRotateStart;
        GameplayInput.Instance.OnItemRotateCanceled -= OnRotateEnd;
    }

    public void DropDragged()
    {
        if (m_HeldObject != null)
            OnPickupEnd();
    }

    private void TryStartDragging()
    {
        if (!m_IsHolding)
        {
            if (Player.Instance.Casting.TargetHasTag(m_DragTagLight, out GameObject targetLight))
            {
                OnPickupLight(targetLight.transform);
            }
            else if (Player.Instance.Casting.TargetHasTag(m_DragTagHeavy, out GameObject targetHeavy))
            {
                OnPickupHeavy(targetHeavy.transform);
            }
        }
    }

    private void CalculateMinDistance(float minDistance)
    {
        if (Physics.Raycast(m_RaycastOrigin.position, m_RaycastOrigin.forward, out RaycastHit hit, m_MaxDistance, ~LayerMask.GetMask("Player", "Ignore Raycast")))
        {
            m_MinDistance = minDistance + Vector3.Distance(hit.point, m_HeldObject.position);
        }

        // m_MinDistance = minDistance + Mathf.Max(m_Collider.bounds.extents.x, Mathf.Max(m_Collider.bounds.extents.y, m_Collider.bounds.extents.z));
    }

    private void OnPickup(Transform target)
    {
        m_HeldObject = target;
        m_HeldRb = m_HeldObject.GetComponent<Rigidbody>();
        m_Collider = m_HeldObject.GetComponent<Collider>();
        m_HeldObject.tag = "Untagged";
        m_Thrown = false;

        //Calculate current rotation offset
        Quaternion cameraYRotation = Quaternion.Euler(0, m_RaycastOrigin.eulerAngles.y, 0);
        m_HeldObjectRotationOffset = Quaternion.Inverse(cameraYRotation) * m_HeldObject.rotation;

        InteractionUI.Instance.ToggleCrosshair(false);
        OnDragStart?.Invoke();
    }

    private void OnPickupLight(Transform target)
    {
        OnPickup(target);
        CalculateMinDistance(m_MinDistanceLigth);
        m_Guide.localPosition = Vector3.forward * Mathf.Clamp(Vector3.Distance(target.position, m_RaycastOrigin.position), m_MinDistance, m_MaxDistance);

        m_IsHolding = true;
        StartCoroutine(WhileHoldingLight());
    }

    private void OnPickupHeavy(Transform target)
    {
        OnPickup(target);
        CalculateMinDistance(m_MinDistanceHeavy);

        Player.Instance.FpsController.CanSprint = false;
        m_HeldRb.constraints = RigidbodyConstraints.FreezeRotation;
        Player.Instance.FpsController.CanJump = false;

        m_IsHolding = true;
        StartCoroutine(WhileHoldingHeavy());
    }

    private bool CanHoldLightObject()
    {
        return GameplayInput.Instance.Interact &&
               !m_Collider.bounds.Contains(m_PlayerFeet.transform.position - Vector3.up * 0.25f) && // Player is not standing on top
               Vector3.Distance(m_RaycastOrigin.position, m_HeldObject.position) <= m_MaxDistance;  // Player is close enough
    }

    private bool CanHoldHeavyObject()
    {
        return CanHoldLightObject() &&
               Physics.Raycast(m_Collider.bounds.center, Vector3.down, out RaycastHit hit, 0.1f + m_Collider.bounds.extents.y + 0.01f); // The heavy object is on the floor
    }

    private Vector3 CalculateTorque()
    {
        Quaternion cameraYRotation = Quaternion.Euler(0, m_RaycastOrigin.eulerAngles.y, 0);
        Quaternion targetRotation = cameraYRotation * m_HeldObjectRotationOffset;
        Quaternion rotationDifference = targetRotation * Quaternion.Inverse(m_HeldRb.rotation);

        rotationDifference.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        if (angleInDegrees > 180)
            angleInDegrees -= 360;

        return rotationAxis * (angleInDegrees * Mathf.Deg2Rad * m_RotationSpring);
    }

    private void HandleObjectPhysics(Vector3 targetPosition, ref Vector3 lastPlayerVelocity, bool rotate)
    {
        lastPlayerVelocity = Vector3.Lerp(lastPlayerVelocity, m_CharacterController.velocity, Time.deltaTime * 10);
        Vector3 targetVelocity = (targetPosition - m_HeldObject.position) * m_DragSpeed / Mathf.Sqrt(m_HeldRb.mass) + lastPlayerVelocity;
        m_HeldRb.linearVelocity = targetVelocity;
        m_HeldRb.angularVelocity = Vector3.zero;

        if (rotate)
            m_HeldRb.AddTorque(CalculateTorque(), ForceMode.Acceleration);
    }

    private IEnumerator WhileHoldingLight()
    {
        Vector3 lastPlayerVelocity = Vector3.zero;

        while (!m_Thrown && CanHoldLightObject())
        {
            HandleObjectPhysics(m_Guide.position, ref lastPlayerVelocity, true);
            HandleScrollMovement();
            HandleManualRotation();

            yield return null;
        }

        OnPickupEndLight();

        if (m_Thrown)
            StartCoroutine(SetTag());
    }

    private IEnumerator SetTag()
    {
        Transform go = m_HeldObject;
        go.tag = "Untagged";
        yield return new WaitForSeconds(m_PickupCooldown);
        go.tag = "LightInteractable";
    }

    private void OnRotateStart()
    {
        if (!m_IsHolding)
            return;

        Player.Instance.FpsController.CanLook = false;
    }

    private void HandleManualRotation()
    {
        float rotateX = Mathf.Clamp(GameplayInput.Instance.ItemRotate.x * m_ManualRotationSpeed, -m_RotationSpeedClamp, m_RotationSpeedClamp);
        float rotateY = Mathf.Clamp(GameplayInput.Instance.ItemRotate.y * m_ManualRotationSpeed, -m_RotationSpeedClamp, m_RotationSpeedClamp);

        Quaternion deltaRotation = Quaternion.Euler(rotateY, -rotateX, 0);
        m_HeldObjectRotationOffset = deltaRotation * m_HeldObjectRotationOffset;

        if (deltaRotation != Quaternion.identity)
        {
            CalculateMinDistance(m_MinDistanceLigth);
            m_Guide.localPosition = Vector3.Lerp(m_Guide.localPosition, Vector3.forward * m_MinDistance, Time.deltaTime * 5);
        }
    }

    private void OnRotateEnd()
    {
        if (!m_IsHolding)
            return;

        Player.Instance.FpsController.CanLook = true;
        CalculateMinDistance(m_MinDistanceLigth);
    }

    private void HandleScrollMovement()
    {
        float scroll = GameplayInput.Instance.Scroll * 0.1f;
        m_Guide.transform.localPosition = Vector3.forward * Mathf.Clamp(m_Guide.transform.localPosition.z + scroll, m_MinDistance, m_MaxDistance);
    }

    private void ThrowHeldObject()
    {
        if (!m_IsHolding)
            return;

        m_HeldRb.AddForce(m_RaycastOrigin.forward * m_ThrowForce, ForceMode.Force);
        m_Thrown = true;
    }

    private IEnumerator WhileHoldingHeavy()
    {
        Vector3 lastPlayerVelocity = Vector3.zero;
        Vector3 diff = m_HeldObject.position - transform.position;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, m_MaxDistance, ~LayerMask.GetMask("Player", "Ignore Raycast")))
        {
            float delta = m_MinDistanceHeavy - hit.distance;

            if (delta > 0)
            {
                diff += transform.forward * delta;
            }
            else
            {
                OnPickupEndHeavy();
                yield break;
            }
        }

        while (CanHoldHeavyObject())
        {
            Vector3 targetPosition = transform.position + diff;
            targetPosition.y = m_HeldObject.position.y;

            HandleObjectPhysics(targetPosition, ref lastPlayerVelocity, false);
            yield return null;
        }

        OnPickupEndHeavy();
    }

    private void OnPickupEnd()
    {
        m_IsHolding = false;
        m_HeldRb.constraints = RigidbodyConstraints.None;
        m_HeldRb.useGravity = true;
        Player.Instance.FpsController.CanSprint = true;
        Player.Instance.FpsController.CanJump = true;
        InteractionUI.Instance.ToggleCrosshair(true);
        OnDragEnd?.Invoke();
    }

    private void OnPickupEndLight()
    {
        OnPickupEnd();
        m_HeldObject.tag = "LightInteractable";
    }

    private void OnPickupEndHeavy()
    {
        OnPickupEnd();
        m_HeldObject.tag = "HeavyInteractable";
    }
}
