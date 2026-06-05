using System.Collections;
using UnityEngine;

public class InteractableAudio : MonoBehaviour
{
    [SerializeField] private AudioLibrary m_Library;

    private bool m_CanPlaySound = false;

    private void Awake()
    {
        StartCoroutine(WaitOneSec());
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (m_CanPlaySound)
            AudioManager.Instance.PlayRandom(m_Library.Category, collision.transform.position);
    }

    private IEnumerator WaitOneSec()
    {
        yield return new WaitForSeconds(1);
        m_CanPlaySound = true;
    }
}
