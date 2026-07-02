using UnityEngine;
using TMPro;
using System.Collections;

public class NowPlayingGUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI trackNameText;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 3.5f;
    [SerializeField] private float fadeSpeed = 2f;

    private CanvasGroup canvasGroup;
    private Coroutine displayCoroutine;
    private bool isSubscribed = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        
        TrySubscribe();

        
        if (MusicManager.Instance != null && MusicManager.Instance.CurrentTrack != null)
        {
            HandleTrackChanged(MusicManager.Instance.CurrentTrack);
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (MusicManager.Instance != null && isSubscribed)
        {
            MusicManager.Instance.OnTrackChanged -= HandleTrackChanged;
            isSubscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (!isSubscribed && MusicManager.Instance != null)
        {
            MusicManager.Instance.OnTrackChanged += HandleTrackChanged;
            isSubscribed = true;
        }
    }

    private void HandleTrackChanged(TrackData track)
    {
        if (track == null || trackNameText == null) return;

        if (displayCoroutine != null) StopCoroutine(displayCoroutine);
        displayCoroutine = StartCoroutine(ShowTrackNotification(track.trackName));
    }

    private IEnumerator ShowTrackNotification(string trackName)
    {
        trackNameText.text = $"Now Playing: {trackName}";

        
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        
        yield return new WaitForSeconds(displayDuration);

        
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}