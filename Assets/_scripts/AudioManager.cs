using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Master / Individual Volumes")]
    [Range(0f, 2f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float BgmVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float SfxVolume = 1f;

    [Header("BGM")]
    [SerializeField] private List<BGMTrack> bgmTracks;
    [SerializeField] private int startingBGMIndex = 0;

    [Header("SFX")]
    [SerializeField] private List<SFXGroup> sfxGroups;
    [SerializeField] private int sfxPoolSize = 10;

    private AudioSource bgmSource;
    private BGMTrack currentBGM;
    private List<AudioSource> sfxPool;
    private Dictionary<string, SFXGroup> sfxDict;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create BGM AudioSource
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f; // 2D audio

        // Build SFX dictionary
        sfxDict = new Dictionary<string, SFXGroup>();
        foreach (var group in sfxGroups)
        {
            if (group == null || group.clips.Count == 0) continue;
            sfxDict[group.name] = group; // use ScriptableObject name
        }

        // Build SFX pool
        sfxPool = new List<AudioSource>();
        for (int i = 0; i < sfxPoolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f; // 2D audio
            sfxPool.Add(src);
        }

        // Start initial BGM
        if (bgmTracks != null && bgmTracks.Count > 0)
            PlayBGM(startingBGMIndex);
    }

    // -------------------- MASTER / BGM / SFX VOLUME --------------------
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetBGMVolume(float volume)
    {
        BgmVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        // Update BGM
        if (bgmSource != null && currentBGM != null)
            bgmSource.volume = currentBGM.volume * BgmVolume * masterVolume;

        // Update all currently playing SFX
        foreach (var src in sfxPool)
            if (src.isPlaying)
                src.volume = src.volume * SfxVolume * masterVolume;
    }

    // -------------------- BGM --------------------
    public void PlayBGM(int index)
    {
        if (bgmTracks == null || bgmTracks.Count == 0) return;
        if (index < 0 || index >= bgmTracks.Count) return;

        var track = bgmTracks[index];
        if (track == null || track.clip == null) return;
        if (bgmSource.clip == track.clip) return;

        bgmSource.clip = track.clip;
        bgmSource.loop = track.loop;
        bgmSource.volume = track.volume * BgmVolume * masterVolume;
        bgmSource.Play();

        currentBGM = track;
    }

    public void StopBGM()
    {
        bgmSource.Stop();
        currentBGM = null;
    }

    // -------------------- SFX --------------------
    public void PlaySFX(string groupName, bool playInstantly = false, float minDelay = 0f, float maxDelay = 0.05f)
    {
        if (!sfxDict.TryGetValue(groupName, out var group)) return;

        var clip = group.GetRandomClip();
        if (clip == null) return;

        var src = GetAvailableSource();
        src.clip = clip;
        src.volume = group.volume * SfxVolume * masterVolume;
        src.pitch = group.GetRandomPitch();
        if (playInstantly)
        {
            src.Play(); // ignore delay
        }
        else
        {
            // tiny random delay to prevent overlapping
            float delay = Random.Range(minDelay, maxDelay);
            if (delay > 0f)
                src.PlayDelayed(delay);
            else
                src.Play();
        }
    }


    private AudioSource GetAvailableSource()
    {
        foreach (var src in sfxPool)
            if (!src.isPlaying) return src;

        return sfxPool[0]; // reuse oldest if all busy
    }
}
