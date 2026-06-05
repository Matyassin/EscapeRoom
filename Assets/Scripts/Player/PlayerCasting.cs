using UnityEngine;

public class PlayerCasting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform m_RaycastOrigin;

    [Header("Settings")]
    [SerializeField, Range(0.1f, 100f)] private float m_Distance = 2f;
    [SerializeField] private LayerMask m_IgnoreMask;

    [SerializeField] private GameObject m_Target = null;
    public RaycastHit Hit { get; private set; }
    public GameObject Target => m_Target;

    private void Update()
    {
        if (Physics.Raycast(m_RaycastOrigin.position, m_RaycastOrigin.forward, out RaycastHit hit, m_Distance, ~m_IgnoreMask))
        {
            m_Target = hit.transform.gameObject;
            Hit = hit;
        }
        else
        {
            m_Target = null;
        }
    }

    public bool HasTarget()
    {
        return m_Target != null;
    }

    public bool HasTarget(out GameObject target)
    {
        target = m_Target;
        return m_Target != null;
    }

    public bool TargetIs(GameObject go)
    {
        return m_Target == go;
    }

    public bool TargetHasTag(string tag)
    {
        return HasTarget() && m_Target.CompareTag(tag);
    }

    public bool TargetHasTag(string tag, out GameObject target)
    {
        target = m_Target;
        return TargetHasTag(tag);
    }

    public string TargetsTag()
    {
        if (HasTarget())
        {
            return m_Target.tag;
        }

        return null;
    }
}
