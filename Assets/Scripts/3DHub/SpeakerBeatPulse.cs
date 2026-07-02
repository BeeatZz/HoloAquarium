using UnityEngine;

/// <summary>
/// Attach to the speaker's visual mesh/model. Makes the speaker scale-pulse in time with the
/// current track's BPM. Pure transform math, no animation asset needed.
/// </summary>
public class SpeakerBeatPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseAmount = 0.08f;   // how much it scales up on each beat
    public float beatSubdivision = 1f;  // 1 = pulse every beat

    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void Update()
    {
        float songTime = Time.time;
        float currentBpm = 120f; // Default fallback

        // Directly read from the manager every frame to avoid execution order bugs
        if (MusicManager.Instance != null && MusicManager.Instance.CurrentTrack != null)
        {
            songTime = MusicManager.Instance.SongTime;
            currentBpm = Mathf.Max(1f, MusicManager.Instance.CurrentTrack.bpm);
        }

        // Convert song timeline seconds directly into beats elapsed
        float beatsPerSecond = currentBpm / 60f;
        float totalBeatsElapsed = songTime * beatsPerSecond * beatSubdivision;

        // Drive sine wave directly from beat progress
        float pulse = Mathf.Abs(Mathf.Sin(totalBeatsElapsed * Mathf.PI)) * pulseAmount;

        // Update the visual scale
        transform.localScale = baseScale + Vector3.one * pulse;
    }
}