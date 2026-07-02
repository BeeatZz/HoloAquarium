using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central music player for the hub scene. Plays TrackData clips through a
/// local AudioSource and broadcasts OnTrackChanged so any listener (like
/// HubGremurin) can react without needing a direct reference back.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    public event Action<TrackData> OnTrackChanged;

    [Header("Playlist")]
    public TrackData[] fullPlaylist; // every track that exists, including locked ones
    private List<TrackData> activePlaylist = new List<TrackData>();

    [Header("Playback")]
    [Range(0f, 1f)] public float volume = 0.6f;
    public bool autoPlayOnStart = true;

    private AudioSource audioSource;
    private TrackData currentTrack;

    public TrackData CurrentTrack => currentTrack;

    // Exposes the current playback time of the audio source in seconds for rhythm sync
    public float SongTime => audioSource != null ? audioSource.time : 0f;

    private void Awake()
    {
        // Simple singleton - if you already have a broader service locator /
        // bootstrap pattern elsewhere, swap this for that instead.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
    }

    private void Start()
    {
        RebuildActivePlaylist();

        if (autoPlayOnStart && activePlaylist.Count > 0)
            PlayTrack(activePlaylist[0]);
    }

    /// <summary>
    /// Filters fullPlaylist down to tracks that are either unlockedByDefault
    /// or present in the player's save data. Call again after a new unlock
    /// (e.g. from LevelCompleteUI) if you want it reflected immediately.
    /// </summary>
    public void RebuildActivePlaylist()
    {
        activePlaylist.Clear();
        if (fullPlaylist == null) return;

        SaveData save = SaveManager.Instance != null ? SaveManager.Instance.GetSaveData() : null;

        foreach (var track in fullPlaylist)
        {
            if (track == null) continue;

            bool unlocked = track.unlockedByDefault
                || (save != null && save.IsTrackUnlocked(track.trackName));

            if (unlocked)
                activePlaylist.Add(track);
        }
    }

    private void Update()
    {
        // auto-advance to next track when current one finishes
        if (currentTrack != null && !audioSource.isPlaying && audioSource.time == 0f)
        {
            PlayNext();
        }
    }

    public void PlayTrack(TrackData track)
    {
        if (track == null || track.clip == null) return;

        currentTrack = track;
        audioSource.clip = track.clip;
        audioSource.volume = volume;
        audioSource.Play();

        OnTrackChanged?.Invoke(track);
    }

    public void PlayNext()
    {
        if (activePlaylist.Count == 0) return;

        int currentIndex = activePlaylist.IndexOf(currentTrack);
        int nextIndex = (currentIndex + 1) % activePlaylist.Count;
        PlayTrack(activePlaylist[nextIndex]);
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
    }
}