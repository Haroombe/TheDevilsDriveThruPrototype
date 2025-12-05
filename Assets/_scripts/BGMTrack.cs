using UnityEngine;

[CreateAssetMenu(fileName = "NewBGM", menuName = "Audio/BGM Track")]
public class BGMTrack : ScriptableObject
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = true;
}
