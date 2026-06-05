using System.Collections;
using UnityEngine;

public class DragHingeInteractable : MonoBehaviour
{
    [Header("Drag Options")]
    [Tooltip("Increasing this results in the player applying more force to HingeInteractables.")]
    [SerializeField, Range(1f, 100f)] private float m_DragStrengthMultiplier = 3f;

    [Tooltip("Specifies the maximum distance from the interactable at which the player loses hold of it.")]
    [SerializeField, Range(3f, 6f)] private float m_MaxDragDistance = 4f;

    private GameObject m_CurrentInteractable;
    private Coroutine m_DragRoutine;

    private bool m_UsesGravity = false;
    public bool IsDragging { get; private set; } = false;

    private void Start()
    {
        GameplayInput.Instance.OnInteractStarted += OnInteract;
        GameplayInput.Instance.OnInteractCanceled += OnInteractCancelled;
    }

    private void OnDestroy()
    {
        GameplayInput.Instance.OnInteractStarted -= OnInteract;
        GameplayInput.Instance.OnInteractCanceled -= OnInteractCancelled;
    }

    private void OnInteract()
    {
        if (!IsDragging && Player.Instance.Casting.TargetHasTag("HingeInteractable", out GameObject interactable))
        {
            SetDragState(interactable);
            m_DragRoutine = StartCoroutine(RotateAroundHinge(interactable.transform));
        }
    }

    private void OnInteractCancelled()
    {
        if (IsDragging && m_DragRoutine != null)
        {
            StopCoroutine(m_DragRoutine);
            ResetDragState();
        }
    }

    private void SetDragState(GameObject interactable)
    {
        Player.Instance.FpsController.CanLook = false;
        InteractionUI.Instance.ToggleCrosshair(false);
        m_CurrentInteractable = interactable;
        m_CurrentInteractable.tag = "Untagged";
        m_UsesGravity = interactable.GetComponent<Rigidbody>().useGravity;
        m_CurrentInteractable.GetComponent<Rigidbody>().useGravity = false;
        IsDragging = true;
    }

    private void ResetDragState()
    {
        Player.Instance.FpsController.CanLook = true;
        InteractionUI.Instance.ToggleCrosshair(true);
        m_CurrentInteractable.tag = "HingeInteractable";
        m_CurrentInteractable.GetComponent<Rigidbody>().useGravity = m_UsesGravity;
        m_CurrentInteractable = null;
        IsDragging = false;
    }

    private IEnumerator RotateAroundHinge(Transform interactable)
    {
        Rigidbody rb = interactable.GetComponent<Rigidbody>();
        if (!interactable.TryGetComponent<HingeJoint>(out HingeJoint hingeJoint))
            yield return null;

        Vector3 hingePos = interactable.position + hingeJoint.axis;
        hingePos.y = hingePos.z;

        Vector3 playerPos = transform.position;
        playerPos.y = playerPos.z;

        Vector2 hingeForward = Vector2.up - (Vector2)hingePos;

        float dir = 1f;

        if (Vector3.Distance(hingePos, playerPos) > 1.5f && (hingeJoint.anchor.x != 0 || hingeJoint.anchor.z != 0))
        {
            dir = Mathf.Sign(Vector3.Dot(hingeForward, playerPos - hingePos));
        }

        while (Vector3.Distance(transform.position, interactable.position) < m_MaxDragDistance && hingeJoint != null)
        {
            Vector2 lookInput = new(
                GameplayInput.Instance.Look.x, // * SettingsManager.Instance.ControlsSettings.LookSensitivity,
                GameplayInput.Instance.Look.y  // * SettingsManager.Instance.ControlsSettings.LookSensitivity
            );

            float verticalInput = lookInput.y;
            float horizontalInput = lookInput.x * hingeJoint.axis.y * dir;

            float force = (verticalInput + horizontalInput) * m_DragStrengthMultiplier;
            Vector3 torque = hingeJoint.axis * force;

            rb.AddRelativeTorque(torque, ForceMode.Force);

            yield return null;
        }

        ResetDragState();
    }
}
