using UnityEngine;

public class StorageEnterTrigger : MonoBehaviour
{
    [SerializeField] private HingeJoint m_Joint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            JointLimits limits = new()
            {
                min = 0,
                max = 90,
                bounceMinVelocity = 0.2f
            };

            m_Joint.limits = limits;
        }
    }
}
