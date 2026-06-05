using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Scriptable Objects/AudioLibrary")]
public class AudioLibrary : ScriptableObject
{
    public AudioCategory Category;
    public PlayAudio[] Clips;

    public PlayAudio GetRandom()
    {
        if (Clips == null)
            return default;

        return Clips[UnityEngine.Random.Range(0, Clips.Length)];
    }
}

public enum AudioCategory
{
    FootstepWood,
    FootstepStone,
    PlayerBreathing,
    Insanity,
    Ambience,
    Music,
    Monster,
    InteractableMetal,
    InteractableWood,
    InteractableGlass,
    DoorCreak,
    DoorClose,
}

[System.Serializable]
public struct PlayAudio
{
    public AudioClip Clip;
    [Range(0, 1)] public float Volume;
}
