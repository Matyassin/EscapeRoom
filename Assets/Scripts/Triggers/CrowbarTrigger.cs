using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CrowbarTrigger : MonoBehaviour
{
    [Header("Collider To Check For")]
    [SerializeField] private Collider m_CrowbarCollider;

    [Header("Hinge Crowbar Stuff")]
    [SerializeField] private GameObject m_HingeCrowbar;
    [SerializeField] private GameObject m_LockedDoor;
    [SerializeField] private ParticleSystem m_SmokeParticles;

    private HingeJoint m_CrowbarHinge;
    private bool m_HasOpened = false;

    public void OnTriggerEnter(Collider other)
    {
        if (other == m_CrowbarCollider)
        {
            AudioManager.Instance.PlayRandom(AudioCategory.InteractableMetal, m_CrowbarCollider.transform.position);
            m_CrowbarCollider.gameObject.SetActive(false);
            m_HingeCrowbar.SetActive(true);
        }
    }

    private void Awake()
    {
        m_CrowbarHinge = m_HingeCrowbar.GetComponent<HingeJoint>();
    }

    private void Update()
    {
        if (m_HasOpened)
            return;

        if (m_CrowbarHinge.angle <= m_CrowbarHinge.limits.min + 2f)
        {
            AudioManager.Instance.PlayRandom(AudioCategory.InteractableMetal, m_CrowbarCollider.transform.position);

            HingeJoint hinge = m_LockedDoor.GetComponent<HingeJoint>();
            JointLimits limits = hinge.limits;
            limits.max = 90f;
            hinge.limits = limits;

            Rigidbody rb = m_LockedDoor.GetComponent<Rigidbody>();
            rb.AddTorque(m_LockedDoor.transform.TransformDirection(hinge.axis) * 200f, ForceMode.Force);

            m_CrowbarHinge.breakForce = 0f;
            m_CrowbarHinge.breakTorque = 0f;
            StartCoroutine(ResetCrowbarState());

            m_SmokeParticles.Play();
            m_HasOpened = true;
        }
    }

    private IEnumerator ResetCrowbarState()
    {
        yield return new WaitForSeconds(0.5f);
        m_HingeCrowbar.tag = "Untagged";
    }
}
