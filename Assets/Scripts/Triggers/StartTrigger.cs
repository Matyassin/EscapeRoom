using System;
using System.Collections;
using UnityEngine;

public class StartTrigger : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform m_CameraParent;

    [Header("UI Black Image")]
    [SerializeField] private CanvasGroup m_BlackImageCg;

    private float m_BaseFov = 0;
    private bool m_PulsateFov = false;
    private bool m_HasPlayedInteractHint = false;

    private Tween<CanvasGroup, float> m_CanvasGroupTween;
    private Tween<Transform, Vector3> m_PosTween;
    private Tween<Transform, Quaternion> m_Rot1Tween;
    private Tween<Transform, Quaternion> m_Rot2Tween;
    private Tween<Transform, Quaternion> m_Rot3Tween;

    public static Action OnCutsceneEnd;

    private void Awake()
    {
        m_CanvasGroupTween = Tween<CanvasGroup, float>.Configure()
            .Setter((cg, v) => cg.alpha = v)
            .Start(1)
            .End(0)
            .LerpFunction(Mathf.Lerp)
            .Duration(2)
            .Build();

        m_Rot1Tween = Tween<Transform, Quaternion>.Configure()
            .ForRotation()
            .Start(() => m_CameraParent.transform.rotation)
            .End(Quaternion.Euler(347.89f, 94.53f, 0f))
            .Duration(2f)
            .Build();

        m_Rot2Tween = Tween<Transform, Quaternion>.Configure()
            .ForRotation()
            .Start(() => m_CameraParent.transform.rotation)
            .End(Quaternion.Euler(21.66f, 87.73f, 0.375f))
            .Duration(2f)
            .Build();

        m_Rot3Tween = Tween<Transform, Quaternion>.Configure()
            .ForRotation()
            .Start(() => m_CameraParent.transform.rotation)
            .End(Quaternion.Euler(0, 72.538f, 0f))
            .Duration(2f)
            .Build();

        m_PosTween = Tween<Transform, Vector3>.Configure()
            .ForPosition()
            .Start(() => m_CameraParent.transform.position)
            .End(new Vector3(2.196f, 1.8f, 7.594f))
            .Duration(2f)
            .Build();

        m_BaseFov = Camera.main.fieldOfView;
    }

    private void Start()
    {
        StartCoroutine(StartCutsceneRoutine());
    }

    private void Update()
    {
        HandlePulsatingCamera();
        CheckForInteractable();
    }

    private void HandlePulsatingCamera()
    {
        if (!m_PulsateFov)
            return;

        float fovOffset = MathF.Sin(Time.time * 2f) * 2f;
        Camera.main.fieldOfView = m_BaseFov + fovOffset;
    }

    private void CheckForInteractable()
    {
        if (m_HasPlayedInteractHint)
            return;

        if (Player.Instance.Casting.TargetHasTag("LightInteractable") || Player.Instance.Casting.TargetHasTag("HingeInteractable"))
        {
            Hints.Instance.ShowHint(HintType.Interact, 4);
            m_HasPlayedInteractHint = true;
        }
    }

    private IEnumerator StartCutsceneRoutine()
    {
        m_PulsateFov = true;
        GhostingFeature.RuntimeBlendAmount = 0.85f;
        m_BlackImageCg.alpha = 1;

        Player.Instance.FpsController.ToggleController(false);
        Player.Instance.CameraFollowPlayer.CanFollowHead = false;
        
        m_CameraParent.SetPositionAndRotation(
            new Vector3(2.19f, 0.3f, 7.59f),
            Quaternion.Euler(0, 72.538f, 5)
        );

        AudioManager.Instance.PlayRandom(AudioCategory.Insanity);
        AudioManager.Instance.PlayRandom(AudioCategory.PlayerBreathing);
        StartCoroutine(m_CanvasGroupTween.AsRoutine(m_BlackImageCg));

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(m_Rot1Tween.AsRoutine(m_CameraParent));
        yield return new WaitForSeconds(2f);
        AudioManager.Instance.PlayRandom(AudioCategory.PlayerBreathing);
        AudioManager.Instance.PlayRandomFadeInOut(AudioCategory.Monster, 0.5f);
        yield return StartCoroutine(m_Rot2Tween.AsRoutine(m_CameraParent));
        yield return new WaitForSeconds(0.25f);

        Coroutine pos = StartCoroutine(m_PosTween.AsRoutine(m_CameraParent));
        Coroutine rot = StartCoroutine(m_Rot3Tween.AsRoutine(m_CameraParent));

        yield return pos;
        yield return rot;

        Player.Instance.CameraFollowPlayer.CanFollowHead = true;
        Player.Instance.FpsController.ToggleController(true);
        AudioManager.Instance.PlayRandom(AudioCategory.PlayerBreathing);

        OnCutsceneEnd?.Invoke();
        Hints.Instance.ShowHint(HintType.Look, 4);
        Hints.Instance.ShowHint(HintType.Move, 4);

        yield return new WaitForSeconds(2f);
        AudioManager.Instance.PlayRandom(AudioCategory.PlayerBreathing);
        yield return new WaitForSeconds(2f);
        AudioManager.Instance.PlayRandom(AudioCategory.PlayerBreathing);
        m_PulsateFov = false;

        yield return new WaitForSeconds(7.5f);

        yield return StartCoroutine(Utilities.InterpolateByValueScaled(
            v => GhostingFeature.RuntimeBlendAmount = v,
            GhostingFeature.RuntimeBlendAmount,
            0,
            10f,
            Mathf.Lerp)
        );
    }
}
