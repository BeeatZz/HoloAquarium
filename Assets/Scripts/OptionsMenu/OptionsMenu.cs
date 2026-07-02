using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Brillo")]
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private CanvasGroup brightnessOverlay;

    private void Start()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", 1f);

        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);
        SetBrightness(brightnessSlider.value);
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat(
            "MusicVolume",
            value <= 0.001f ? -80f : Mathf.Log10(value) * 20f
        );

        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat(
            "SFXVolume",
            value <= 0.001f ? -80f : Mathf.Log10(value) * 20f
        );

        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void SetBrightness(float value)
    {
        brightnessOverlay.alpha = 1f - value;
        PlayerPrefs.SetFloat("Brightness", value);
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();
    }
}
