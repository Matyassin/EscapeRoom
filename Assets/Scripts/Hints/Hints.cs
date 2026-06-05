using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class Hints : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_HintsText;

    private readonly Queue<HintData> m_HintQueue = new();
    private bool m_IsQueueProcessing = false;

    private CanvasGroup m_CanvasGroup;
    private Tween<CanvasGroup, float> m_CanvasGroupTween;

    public static Hints Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        m_CanvasGroup = GetComponent<CanvasGroup>();

        m_CanvasGroupTween = Tween<CanvasGroup, float>.Configure()
            .Setter((cg, v) => cg.alpha = v)
            .Start(0)
            .End(1)
            .Duration(2)
            .LerpFunction(Mathf.Lerp)
            .Build();
    }

    public void ShowHint(HintType type, float duration)
    {
        m_HintQueue.Enqueue(new HintData(type, duration));

        if (!m_IsQueueProcessing)
            StartCoroutine(ProcessQueueRoutine());
    }

    private IEnumerator ProcessQueueRoutine()
    {
        m_IsQueueProcessing = true;

        while (m_HintQueue.Count > 0)
        {
            HintData currentHint = m_HintQueue.Dequeue();

            switch (currentHint.Type)
            {
                case HintType.Move:
                    m_HintsText.text = "You can move using (WASD/LS).";
                    break;

                case HintType.Look:
                    m_HintsText.text = "Use (MOUSE/RS) to look around.";
                    break;

                case HintType.Interact:
                    m_HintsText.text = "You can interact with most items by holding down (LEFT MOUSE BUTTON/RT).";
                    break;
            }

            yield return StartCoroutine(HintsFade(currentHint.Duration));
        }

        m_IsQueueProcessing = false;
    }

    private IEnumerator HintsFade(float duration)
    {
        yield return StartCoroutine(m_CanvasGroupTween.AsRoutine(m_CanvasGroup));
        yield return new WaitForSeconds(duration);
        yield return StartCoroutine(m_CanvasGroupTween.AsRoutineReversed(m_CanvasGroup));
    }

    struct HintData
    {
        public HintType Type;
        public float Duration;

        public HintData(HintType type, float duration)
        {
            Type = type;
            Duration = duration;
        }
    }
}

public enum HintType
{
    Move,
    Look,
    Interact,
}
