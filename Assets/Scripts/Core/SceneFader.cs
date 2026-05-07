using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using DG.Tweening;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [Header("References")]
    public Image fadeImage;

    [Header("Settings")]
    public float fadeDuration = 0.5f;

    private Action onReadyCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        fadeImage.color = new Color(0, 0, 0, 1);
        fadeImage.raycastTarget = true;

        if (onReadyCallback == null)
            FadeIn();
    }


    public void SetReadyCallback(Action callback)
    {
        onReadyCallback = callback;
    }

    public void FadeIn()
    {
        onReadyCallback = null;
        fadeImage.raycastTarget = false;

        fadeImage.DOKill();
        fadeImage
            .DOFade(0f, fadeDuration)
            .SetUpdate(true);
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        fadeImage.raycastTarget = true;
        fadeImage.DOKill();

        yield return fadeImage
            .DOFade(1f, fadeDuration)
            .SetUpdate(true)
            .WaitForCompletion();

        fadeImage.color = new Color(0, 0, 0, 1f);

        SceneManager.LoadScene(sceneName);
    }
}