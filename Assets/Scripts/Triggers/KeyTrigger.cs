using UnityEngine;

public class KeyTrigger : MonoBehaviour
{
    [Header("Collider To Check For")]
    [SerializeField] private Collider m_KeyCollider;

    [SerializeField] private GameObject m_LockedDoor;
    [SerializeField] private GameObject m_Key;

    private void OnTriggerEnter(Collider other)
    {
        if (other == m_KeyCollider)
        {
            HingeJoint hinge = m_LockedDoor.GetComponent<HingeJoint>();
            JointLimits limits = hinge.limits;
            limits.max = 90f;
            hinge.limits = limits;

            Rigidbody rb = m_LockedDoor.GetComponent<Rigidbody>();
            rb.AddTorque(m_LockedDoor.transform.TransformDirection(hinge.axis) * 200f, ForceMode.Force);

            Player.Instance.DragRigidbody.DropDragged();
            m_Key.SetActive(false);
        }
    }
}
