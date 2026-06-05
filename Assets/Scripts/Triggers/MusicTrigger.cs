using System.Collections;
using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    private void Start()
    {
        StartTrigger.OnCutsceneEnd += PlayMusic;
    }

    private void OnDestroy()
    {
        StartTrigger.OnCutsceneEnd -= PlayMusic;
    }

    private void PlayMusic()
    {
        AudioManager.Instance.PlayLoopingFadeIn(AudioCategory.Music, 4);
        StartCoroutine(PlayAmbientOnCooldown());
    }

    private IEnumerator PlayAmbientOnCooldown()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(30, 60));
            AudioManager.Instance.PlayRandomFadeInOut(AudioCategory.Ambience, 2);
        }
    }
}
