using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSFXGroup", menuName = "Audio/SFX Group")]
public class SFXGroup : ScriptableObject
{
    public List<AudioClip> clips;       // One or more clips for variety
    [Range(0f, 1f)] public float volume = 1f;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Count == 0) return null;
        return clips[Random.Range(0, clips.Count)];
    }
}
