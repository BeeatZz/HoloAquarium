using UnityEngine;





public class SpeakerBeatPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseAmount = 0.08f;   
    public float beatSubdivision = 1f;  

    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void Update()
    {
        float songTime = Time.time;
        float currentBpm = 120f; 

        
        if (MusicManager.Instance != null && MusicManager.Instance.CurrentTrack != null)
        {
            songTime = MusicManager.Instance.SongTime;
            currentBpm = Mathf.Max(1f, MusicManager.Instance.CurrentTrack.bpm);
        }

        
        float beatsPerSecond = currentBpm / 60f;
        float totalBeatsElapsed = songTime * beatsPerSecond * beatSubdivision;

        
        float pulse = Mathf.Abs(Mathf.Sin(totalBeatsElapsed * Mathf.PI)) * pulseAmount;

        
        transform.localScale = baseScale + Vector3.one * pulse;
    }
}