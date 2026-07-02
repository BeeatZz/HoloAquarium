using UnityEngine;

public class PersistentSettings : MonoBehaviour
{
    public static PersistentSettings Instance;

    [SerializeField] private CanvasGroup brightnessOverlay;

    private void Awake()
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        float brightness = PlayerPrefs.GetFloat("Brightness", 1f);
        SetBrightness(brightness);
    }

    public void SetBrightness(float value)
    {
        brightnessOverlay.alpha = 1f - value;
        PlayerPrefs.SetFloat("Brightness", value);
        PlayerPrefs.Save();
    }
}