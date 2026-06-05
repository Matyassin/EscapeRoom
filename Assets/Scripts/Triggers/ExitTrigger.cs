using System;
using System.Collections;
using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    [SerializeField] private GameObject m_ExitDoor;

    private HingeJoint m_DoorHinge;
    private float m_BaseFov = 0;
    private bool m_PulsateFov = false;
    private bool m_HasTriggered = false;

    private void Awake()
    {
        m_DoorHinge = m_ExitDoor.GetComponent<HingeJoint>();
        m_BaseFov = Camera.main.fieldOfView;
    }

    private void Update()
    {
        if (!m_PulsateFov)
            return;

        float fovOffset = MathF.Sin(Time.time * 2f) * 2f;
        Camera.main.fieldOfView = m_BaseFov + fovOffset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_HasTriggered)
            return;

        if (other.CompareTag("Player"))
        {
            m_HasTriggered = true;

            Player.Instance.FpsController.CanSprint = false;
            Player.Instance.FpsController.m_MoveSpeed /= 4;

            StartCoroutine(Hallucinate());
        }
    }

    private IEnumerator Hallucinate()
    {
        yield return StartCoroutine(m_ExitDoor.transform.InterpolateByScaled(
            (t, r) => t.localRotation = r,
            m_ExitDoor.transform.localRotation,
            Quaternion.Euler(0f, 0f, 0f),
            Quaternion.Slerp,
            0.1f
        ));

        JointLimits limits = m_DoorHinge.limits;
        limits.max = 1f;
        m_DoorHinge.limits = limits;

        AudioManager.Instance.PlayRandom(AudioCategory.Insanity);
        AudioManager.Instance.PlayRandom(AudioCategory.PlayerBreathing);

        m_PulsateFov = true;

        yield return StartCoroutine(Utilities.InterpolateByValueScaled(
            v => GhostingFeature.RuntimeBlendAmount = v,
            0,
            0.85f,
            10f,
            Mathf.Lerp)
        );

        while (true)
        {
            AudioManager.Instance.PlayRandom(AudioCategory.Insanity);
            AudioManager.Instance.PlayRandom(AudioCategory.PlayerBreathing);

            yield return new WaitForSeconds(UnityEngine.Random.Range(15f, 20f));
        }
    }
}
