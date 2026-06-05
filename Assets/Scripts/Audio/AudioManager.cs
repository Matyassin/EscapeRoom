using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private GameObject m_AudioSourcePrefab;
    [SerializeField] private Transform m_Parent;
    [SerializeField] private AudioLibrary[] m_Libraries;

    private Dictionary<AudioCategory, AudioLibrary> m_LibraryDict;

    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        m_LibraryDict = new Dictionary<AudioCategory, AudioLibrary>();

        foreach (var library in m_Libraries)
        {
            if (!m_LibraryDict.ContainsKey(library.Category))
                m_LibraryDict.Add(library.Category, library);
        }
    }

    public void PlayLooping(AudioCategory category, Vector3? position = null)
    {
        if (!m_LibraryDict.TryGetValue(category, out AudioLibrary library))
            return;

        PlayAudio audio = library.GetRandom();
        Vector3 spawnPos = position ?? Vector3.zero;
        AudioSource source = Instantiate(m_AudioSourcePrefab, spawnPos, Quaternion.identity, m_Parent).GetComponent<AudioSource>();

        source.clip = audio.Clip;
        source.volume = audio.Volume;
        source.loop = true;
        source.spatialBlend = position.HasValue ? 1f : 0f;
        source.Play();

        //Destroy(source.gameObject, audio.Clip.length);
    }

    public void PlayLoopingFadeIn(AudioCategory category, float fadeDuration = 1, Vector3? position = null)
    {
        if (!m_LibraryDict.TryGetValue(category, out AudioLibrary library))
            return;

        PlayAudio audio = library.GetRandom();
        Vector3 spawnPos = position ?? Vector3.zero;
        AudioSource source = Instantiate(m_AudioSourcePrefab, spawnPos, Quaternion.identity, m_Parent).GetComponent<AudioSource>();

        source.clip = audio.Clip;
        source.volume = 0f;
        source.loop = true;
        source.spatialBlend = position.HasValue ? 1f : 0f;
        source.Play();

        StartCoroutine(source.InterpolateByScaled(
            (s, v) => s.volume = v,
            0f,
            audio.Volume,
            Mathf.Lerp,
            fadeDuration
            //() => Destroy(source.gameObject, audio.Clip.length)
            )
        );
    }


    public void PlayRandom(AudioCategory category, Vector3? position = null)
    {
        if (!m_LibraryDict.TryGetValue(category, out AudioLibrary library))
            return;

        PlayAudio audio = library.GetRandom();
        Vector3 spawnPos = position ?? Vector3.zero;
        AudioSource source = Instantiate(m_AudioSourcePrefab, spawnPos, Quaternion.identity, m_Parent).GetComponent<AudioSource>();

        source.clip = audio.Clip;
        source.volume = audio.Volume;
        source.spatialBlend = position.HasValue ? 1f : 0f;
        source.Play();

        Destroy(source.gameObject, audio.Clip.length);
    }

    public void PlayRandomFadeIn(AudioCategory category, float fadeDuration = 1f, Vector3? position = null)
    {
        if (!m_LibraryDict.TryGetValue(category, out AudioLibrary library))
            return;

        PlayAudio audio = library.GetRandom();
        Vector3 spawnPos = position ?? Vector3.zero;
        AudioSource source = Instantiate(m_AudioSourcePrefab, spawnPos, Quaternion.identity, m_Parent).GetComponent<AudioSource>();

        source.clip = audio.Clip;
        source.volume = 0f;
        source.spatialBlend = position.HasValue ? 1f : 0f;
        source.Play();

        StartCoroutine(source.InterpolateByScaled(
            (s, v) => s.volume = v,
            0f,
            audio.Volume,
            Mathf.Lerp,
            fadeDuration,
            () => Destroy(source.gameObject, audio.Clip.length))
        );
    }

    public void PlayRandomFadeOut(AudioCategory category, float fadeDuration = 1f, Vector3? position = null)
    {
        if (!m_LibraryDict.TryGetValue(category, out AudioLibrary library))
            return;

        PlayAudio audio = library.GetRandom();
        Vector3 spawnPos = position ?? Vector3.zero;
        AudioSource source = Instantiate(m_AudioSourcePrefab, spawnPos, Quaternion.identity, m_Parent).GetComponent<AudioSource>();

        source.clip = audio.Clip;
        source.volume = audio.Volume;
        source.spatialBlend = position.HasValue ? 1f : 0f;
        source.Play();

        StartCoroutine(FadeOutRoutine(source, audio, fadeDuration));
    }

    private IEnumerator FadeOutRoutine(AudioSource source, PlayAudio audio, float fadeDuration)
    {
        float waitTime = Mathf.Max(0f, audio.Clip.length - fadeDuration);
        yield return new WaitForSeconds(waitTime);

        if (source != null)
        {
            yield return source.InterpolateByScaled(
                (s, v) => s.volume = v,
                source.volume,
                0f,
                Mathf.Lerp,
                fadeDuration,
                () => Destroy(source.gameObject)
            );
        }
    }

    public void PlayRandomFadeInOut(AudioCategory category, float fadeDuration = 1f, Vector3? position = null)
    {
        if (!m_LibraryDict.TryGetValue(category, out AudioLibrary library))
            return;

        PlayAudio audio = library.GetRandom();
        Vector3 spawnPos = position ?? Vector3.zero;
        AudioSource source = Instantiate(m_AudioSourcePrefab, spawnPos, Quaternion.identity, m_Parent).GetComponent<AudioSource>();

        source.clip = audio.Clip;
        source.volume = 0f;
        source.spatialBlend = position.HasValue ? 1f : 0f;
        source.Play();

        StartCoroutine(FadeInOutRoutine(source, audio, fadeDuration));
    }

    private IEnumerator FadeInOutRoutine(AudioSource source, PlayAudio audio, float fadeDuration)
    {
        float safeFade = Mathf.Min(fadeDuration, audio.Clip.length / 2f);
        float middleWaitTime = Mathf.Max(0f, audio.Clip.length - (safeFade * 2f));

        yield return source.InterpolateByScaled(
            (s, v) => s.volume = v,
            0f,
            audio.Volume,
            Mathf.Lerp,
            safeFade
        );

        if (middleWaitTime > 0)
            yield return new WaitForSeconds(middleWaitTime);

        yield return source.InterpolateByScaled(
            (s, v) => s.volume = v,
            source.volume,
            0f,
            Mathf.Lerp,
            safeFade,
            () => Destroy(source.gameObject)
        );
    }
}
