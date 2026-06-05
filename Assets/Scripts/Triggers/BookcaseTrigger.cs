using UnityEngine;

public class BookcaseTrigger : MonoBehaviour
{
    [SerializeField] private Transform m_BookTransform;
    [SerializeField] private Transform m_Bookcase;

    private Tween<Transform, Vector3> m_BookcaseTween;
    private ConfigurableJoint m_BookJoint;
    private Rigidbody m_BookRb;
    private Vector3 m_StartPos;
    private bool m_PulledBook = false;

    private void Awake()
    {
        m_StartPos = m_BookTransform.localPosition;
        m_BookJoint = m_BookTransform.GetComponent<ConfigurableJoint>();
        m_BookRb = m_BookTransform.GetComponent<Rigidbody>();

        m_BookcaseTween = Tween<Transform, Vector3>.Configure()
            .ForPosition()
            .Start(m_Bookcase.position)
            .End(new Vector3(-13.01f, -0.007f, 8.368f))
            .Duration(4)
            .Build();
    }

    private void Update()
    {
        if (m_PulledBook)
            return;

        if (IsAtLimit())
        {
            m_PulledBook = true;
            m_BookRb.isKinematic = true;
            // play audio
            StartCoroutine(m_BookcaseTween.AsRoutine(m_Bookcase));
        }
    }

    private bool IsAtLimit()
    {
        Vector3 localDisplacement = m_BookTransform.localPosition - m_StartPos;
        float currentDistance = Vector3.Dot(localDisplacement, m_BookJoint.axis);
        float absDistance = Mathf.Abs(currentDistance);

        return absDistance >= (m_BookJoint.linearLimit.limit - 0.005f);
    }
}
