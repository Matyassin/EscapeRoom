using System.Collections;
using UnityEngine;

public class DragDrawer : MonoBehaviour
{
    [Header("Drag Options")]
    [Tooltip("Controls how fast the drawer moves in response to player input.")]
    [SerializeField, Range(0.1f, 5f)] private float m_DragStrength = 1.0f;

    [Tooltip("Specifies the maximum speed at which the drawer can move in response to player input.")]
    [SerializeField, Range(1f, 10f)] private float m_MaxDragSpeed = 5.0f;

    [Tooltip("Specifies the maximum distance from the drawer at which the player loses hold of it.")]
    [SerializeField, Range(3f, 6f)] private float m_MaxDragDistance = 4f;

    private GameObject m_CurrentDrawer;
    private Coroutine m_DragRoutine;

    public bool IsDragging { get; private set; } = false;

    private void Start()
    {
        GameplayInput.Instance.OnInteractStarted += TryStartDragging;
        GameplayInput.Instance.OnInteractCanceled += StopDragging;
    }

    private void OnDestroy()
    {
        GameplayInput.Instance.OnInteractStarted -= TryStartDragging;
        GameplayInput.Instance.OnInteractCanceled -= StopDragging;
    }

    private void TryStartDragging()
    {
        if (!IsDragging && Player.Instance.Casting.TargetHasTag("Drawer", out GameObject drawer))
        {
            SetDrawerStateToDragging(drawer);
            m_DragRoutine = StartCoroutine(WhileDraggingDrawer(drawer.transform));
        }
    }

    private void StopDragging()
    {
        if (IsDragging && m_DragRoutine != null)
        {
            StopCoroutine(m_DragRoutine);
            ResetDrawerState();
        }
    }

    private void SetDrawerStateToDragging(GameObject drawer)
    {
        InteractionUI.Instance.ToggleCrosshair(false);
        Player.Instance.FpsController.CanLook = false;
        m_CurrentDrawer = drawer;
        m_CurrentDrawer.tag = "Untagged";
        IsDragging = true;
    }

    private void ResetDrawerState()
    {
        InteractionUI.Instance.ToggleCrosshair(true);
        Player.Instance.FpsController.CanLook = true;
        m_CurrentDrawer.tag = "Drawer";
        m_CurrentDrawer = null;
        IsDragging = false;
    }

    private IEnumerator WhileDraggingDrawer(Transform drawer)
    {
        Rigidbody rb = drawer.GetComponent<Rigidbody>();

        while (Vector3.Distance(transform.position, drawer.position) < m_MaxDragDistance)
        {
            Vector2 lookInput = new(
                GameplayInput.Instance.Look.x,   // * SettingsManager.Instance.ControlsSettings.LookSensitivity,
                GameplayInput.Instance.Look.y);  // * SettingsManager.Instance.ControlsSettings.LookSensitivity);

            Vector3 lookAxis = new(lookInput.x, 0, -lookInput.y);
            Vector3 relDir = transform.TransformDirection(drawer.forward); //change it to forward later

            float dot = Vector3.Dot(relDir, lookAxis);
            float dragForce = Mathf.Clamp(dot * m_DragStrength, -m_MaxDragSpeed, m_MaxDragSpeed);

            rb.AddForce(-drawer.forward * dragForce, ForceMode.Acceleration);
            yield return null;
        }

        ResetDrawerState();
    }
}
