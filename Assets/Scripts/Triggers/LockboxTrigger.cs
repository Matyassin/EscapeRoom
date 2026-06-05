using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class LockboxTrigger : MonoBehaviour
{
    [Header("Animation stuff")]
    [SerializeField] private Collider m_BoxCollider;
    [SerializeField] private Transform m_CamParent;
    [SerializeField] private Transform m_TargetTransform;
    [SerializeField] private Transform m_LidTransform;
    [SerializeField] private float m_TransitionDuration = 0.5f;

    private Coroutine m_PosRoutine;
    private Coroutine m_RotRoutine;
    private Vector3 m_SavedLocalPos;
    private Quaternion m_SavedLocalRot;
    private bool m_IsInteracting = false;
    
    [Header("Puzzle")]
    [SerializeField] private GameObject[] m_Dials;
    [SerializeField] private int[] m_CorrectCombination = { 3, 7, 4 };
    [SerializeField] private float m_DialRotateDuration = 0.2f;

    private Camera m_Camera;
    private readonly int[] m_DialValues = new int[] {1, 1, 1};
    private readonly bool[] m_IsDialRotating = new bool[] {false, false, false};
    private bool m_PuzzleCompleted = false;

    private void Awake()
    {
        m_Camera = Camera.main;
    }

    private void Start()
    {
        GameplayInput.Instance.OnThrowStarted += Back;
        GameplayInput.Instance.OnInteractStarted += ChekForInteraction;
    }

    private void OnDestroy()
    {
        GameplayInput.Instance.OnThrowStarted -= Back;
        GameplayInput.Instance.OnInteractStarted -= ChekForInteraction;
    }

    private void ChekForInteraction()
    {
        if (m_PuzzleCompleted)
            return;

        if (!m_IsInteracting)
        {
            if (Player.Instance.Casting.TargetHasTag("Box"))
            {
                EnterLockboxView();
            }
        }
        else
        {
            HandleDialCursorClick();
        }
    }

    private void EnterLockboxView()
    {
        m_IsInteracting = true;

        m_BoxCollider.enabled = false;
        InteractionUI.Instance.ToggleCrosshair(false);
        CursorUI.Instance.ToggleCursor(true);

        Player.Instance.FpsController.ToggleController(false);
        Player.Instance.CameraFollowPlayer.CanFollowHead = false;

        m_SavedLocalPos = m_CamParent.localPosition;
        m_SavedLocalRot = m_CamParent.localRotation;

        StopCameraRoutines();

        m_PosRoutine = StartCoroutine(m_CamParent.InterpolateByScaled(
            (t, v) => t.position = v,
            m_CamParent.position,
            m_TargetTransform.position,
            Vector3.Lerp,
            m_TransitionDuration
        ));

        m_RotRoutine = StartCoroutine(m_CamParent.InterpolateByScaled(
            (t, v) => t.rotation = v,
            m_CamParent.rotation,
            m_TargetTransform.rotation,
            Quaternion.Slerp,
            m_TransitionDuration
        ));
    }

    private void HandleDialCursorClick()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = m_Camera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            for (int i = 0; i < m_Dials.Length; i++)
            {
                if (hit.transform.gameObject == m_Dials[i])
                {
                    RotateDial(i);
                    break;
                }
            }
        }
    }

    private void RotateDial(int dialIndex)
    {
        if (m_IsDialRotating[dialIndex])
            return;

        m_IsDialRotating[dialIndex] = true;

        m_DialValues[dialIndex] = (m_DialValues[dialIndex] + 1) % 10;
        Quaternion targetRot = m_Dials[dialIndex].transform.localRotation * Quaternion.Euler(-36, 0f, 0);

        int index = dialIndex;

        StartCoroutine(m_Dials[index].transform.InterpolateByScaled(
            (t, r) => t.localRotation = r,
            m_Dials[index].transform.localRotation,
            targetRot,
            Quaternion.Slerp,
            m_DialRotateDuration,
            () => OnDialRotationComplete(index)
        ));
    }

    private void OnDialRotationComplete(int index)
    {
        m_IsDialRotating[index] = false;
        CheckCombination();
    }

    private void CheckCombination()
    {
        for (int i = 0; i < m_Dials.Length; i++)
        {
            if (m_DialValues[i] != m_CorrectCombination[i])
                return;
        }

        OnPuzzleSolved();
    }

    private void OnPuzzleSolved()
    {
        m_PuzzleCompleted = true;
        Back();
        StartCoroutine(OpenLockBox());
    }

    private void Back()
    {
        if (!m_IsInteracting)
            return;

        StopCameraRoutines();

        InteractionUI.Instance.ToggleCrosshair(true);
        CursorUI.Instance.ToggleCursor(false);

        Vector3 returnWorldPos = m_CamParent.parent.TransformPoint(m_SavedLocalPos);
        Quaternion returnWorldRot = m_CamParent.parent.rotation * m_SavedLocalRot;

        m_PosRoutine = StartCoroutine(m_CamParent.InterpolateByScaled(
            (t, v) => t.position = v,
            m_CamParent.position,
            returnWorldPos,
            Vector3.Lerp,
            m_TransitionDuration
        ));

        m_RotRoutine = StartCoroutine(m_CamParent.InterpolateByScaled(
            (t, v) => t.rotation = v,
            m_CamParent.rotation,
            returnWorldRot,
            Quaternion.Slerp,
            m_TransitionDuration,
            OnReturnComplete
        ));
    }

    private void OnReturnComplete()
    {
        m_CamParent.SetLocalPositionAndRotation(m_SavedLocalPos, m_SavedLocalRot);

        Player.Instance.FpsController.ToggleController(true);
        Player.Instance.CameraFollowPlayer.CanFollowHead = true;

        if (!m_PuzzleCompleted)
            m_BoxCollider.enabled = true;

        m_IsInteracting = false;
    }

    private void StopCameraRoutines()
    {
        if (m_PosRoutine != null)
            StopCoroutine(m_PosRoutine);

        if (m_RotRoutine != null)
            StopCoroutine(m_RotRoutine);
    }

    private IEnumerator OpenLockBox()
    {
        yield return StartCoroutine(m_LidTransform.InterpolateByScaled(
            (t, r) => t.localRotation = r,
            m_LidTransform.localRotation,
            Quaternion.Euler(-5f, 0f, 0f),
            Quaternion.Slerp,
            0.5f
        ));
    }
}
