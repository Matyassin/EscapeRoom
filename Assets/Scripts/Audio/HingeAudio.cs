using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HingeJoint))]
public class HingeAudio : MonoBehaviour
{
    [Header("Creak Settings")]
    [Tooltip("How fast the door must swing before it starts creaking.")]
    [SerializeField] private float m_MinCreakVelocity = 15f;
    [Tooltip("Time between creak sounds when moving at base speed.")]
    [SerializeField] private float m_CreakCooldown = 0.6f;

    [Header("Close Settings")]
    [Tooltip("How close (in degrees) the door needs to be to its minimum limit to trigger the close sound.")]
    [SerializeField] private float m_AngleThreshold = 1.5f;
    [Tooltip("Minimum angular velocity required to trigger the close click (prevents sounds when barely drifting closed).")]
    [SerializeField] private float m_MinCloseVelocity = 5f;

    [Tooltip("Use this if the close sfx plays at the wrong corner.")]
    [SerializeField] private bool m_PlayCloseAtHingeMax = true;

    private HingeJoint m_Hinge;
    private float m_CreakTimer = 0f;
    private bool m_WasAtCloseLimit = true;
    private bool m_CanPlaySound = false;

    private void Awake()
    {
        m_Hinge = GetComponent<HingeJoint>();
        StartCoroutine(WaitOneSec());
    }

    private void Update()
    {
        HandleCreak();
        HandleCloseLimit();
    }

    private void HandleCreak()
    {
        if (!m_CanPlaySound)
            return;

        float currentVelocity = Mathf.Abs(m_Hinge.velocity);
        if (currentVelocity > m_MinCreakVelocity)
        {
            m_CreakTimer -= Time.deltaTime;

            if (m_CreakTimer <= 0f)
            {
                AudioManager.Instance.PlayRandom(AudioCategory.DoorCreak, transform.position);
                float speedFactor = Mathf.Max(1f, currentVelocity / m_MinCreakVelocity);
                m_CreakTimer = m_CreakCooldown / speedFactor;
            }
        }
        else
        {
            m_CreakTimer = Mathf.MoveTowards(m_CreakTimer, 0f, Time.deltaTime);
        }
    }

    private void HandleCloseLimit()
    {
        if (!m_CanPlaySound || !m_Hinge.useLimits)
            return;

        float targetLimit = m_PlayCloseAtHingeMax ? m_Hinge.limits.max : m_Hinge.limits.min;
        bool isAtCloseLimit = Mathf.Abs(m_Hinge.angle - targetLimit) <= m_AngleThreshold;

        if (isAtCloseLimit)
        {
            if (!m_WasAtCloseLimit && Mathf.Abs(m_Hinge.velocity) >= m_MinCloseVelocity)
            {
                AudioManager.Instance.PlayRandom(AudioCategory.DoorClose, transform.position);
                m_WasAtCloseLimit = true;
                m_CreakTimer = m_CreakCooldown;
            }
        }
        else
        {
            m_WasAtCloseLimit = false;
        }
    }

    private IEnumerator WaitOneSec()
    {
        yield return new WaitForSeconds(1);
        m_CanPlaySound = true;
    }
}
