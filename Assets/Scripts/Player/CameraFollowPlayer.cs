using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform m_HeadTransform;

    [Header("Smoothing Settings")]
    [SerializeField] private SmoothingTypes m_SmoothingType = SmoothingTypes.SmoothDamp;
    [SerializeField, Range(0.05f, 0.5f)] private float m_DampSmoothTime = 0.1f;
    [SerializeField, Range(10f, 100f)] private float m_LerpSpeed = 20f;

    private Vector3 m_PositionVelocity;
    private Vector3 m_RotationVelocity;

    public SmoothingTypes SmoothingType
    {
        get => m_SmoothingType;
        set => m_SmoothingType = value;
    }

    public float MouseSmoothTime { get; set; } = 0.01f;
    public bool CanFollowHead { get; set; } = true;

    private void LateUpdate()
    {
        if (!CanFollowHead)
            return;

        switch (m_SmoothingType)
        {
            case SmoothingTypes.None:
                transform.position = m_HeadTransform.position;
                break;

            case SmoothingTypes.Lerp:
                transform.position = Vector3.Lerp(transform.position, m_HeadTransform.position, Time.deltaTime * m_LerpSpeed);
                break;

            case SmoothingTypes.SmoothDamp:
                transform.position = Vector3.SmoothDamp(transform.position, m_HeadTransform.position, ref m_PositionVelocity, m_DampSmoothTime);
                break;

            case SmoothingTypes.EaseInOut:
                float t = Mathf.SmoothStep(0f, 1f, Time.deltaTime * m_LerpSpeed);
                transform.position = Vector3.Lerp(transform.position, m_HeadTransform.position, t);
                break;
        }

        Vector3 currentEuler = transform.rotation.eulerAngles;
        Vector3 targetEuler = m_HeadTransform.rotation.eulerAngles;

        float x = Mathf.SmoothDampAngle(currentEuler.x, targetEuler.x, ref m_RotationVelocity.x, MouseSmoothTime);
        float y = Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref m_RotationVelocity.y, MouseSmoothTime);
        float z = Mathf.SmoothDampAngle(currentEuler.z, targetEuler.z, ref m_RotationVelocity.z, MouseSmoothTime);

        if (float.IsNaN(x))
        {
            x = currentEuler.x;
            m_RotationVelocity.x = 0f;
        }
        if (float.IsNaN(y))
        {
            y = currentEuler.y;
            m_RotationVelocity.y = 0f;
        }
        if (float.IsNaN(z))
        {
            z = currentEuler.z;
            m_RotationVelocity.z = 0f;
        }

        transform.rotation = Quaternion.Euler(x, y, z);
    }

    public enum SmoothingTypes
    {
        None,
        Lerp,
        SmoothDamp,
        EaseInOut
    }
}
