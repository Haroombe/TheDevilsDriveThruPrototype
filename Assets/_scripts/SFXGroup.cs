using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSFXGroup", menuName = "Audio/SFX Group")]
public class SFXGroup : ScriptableObject
{
    public List<AudioClip> clips;       // One or more clips for variety
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Pitch Randomization")]
    public bool isRandomPitch = false;
    [Range(-3f, 3f)] public float minPitch = 0.8f; // Standard AudioSource limits the pitch to -3 to 3
    [Range(-3f, 3f)] public float maxPitch = 1.2f;
    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Count == 0) return null;
        return clips[Random.Range(0, clips.Count)];
    }

    public float GetRandomPitch()
    {
        if (isRandomPitch)
        {
            // Ensure minPitch is not greater than maxPitch before using Random.Range
            float min = Mathf.Min(minPitch, maxPitch);
            float max = Mathf.Max(minPitch, maxPitch);

            // Random.Range for floats includes both min and max
            return Random.Range(min, max);
        }

        return 1.0f; // Default pitch
    }
}
