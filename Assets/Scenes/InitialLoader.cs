using UnityEngine;
using TMPro; // Replace with 'using UnityEngine.UI;' if using legacy Text
using System.Collections;

public class InitialLoader : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text loadingText; // Change to 'public Text loadingText;' if using legacy UI

    [Header("Configuration")]
    [SerializeField] private string targetSceneName = "MainMenu";
    [SerializeField] private float waitBeforeFade = 2f;
    [SerializeField] private float dotAnimationSpeed = 0.5f;

    private const string BaseText = "Loading";

    private void Start()
    {
        // Start animating the text dots immediately
        StartCoroutine(AnimateLoadingText());

        // Start the countdown to switch scenes
        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator AnimateLoadingText()
    {
        int dotCount = 0;

        while (true)
        {
            // Generates "Loading", "Loading.", "Loading..", or "Loading..."
            loadingText.text = BaseText + new string('.', dotCount);

            yield return new WaitForSeconds(dotAnimationSpeed);

            // Cycle dotCount from 0 to 3, then reset to 0
            dotCount = (dotCount + 1) % 4;
        }
    }

    private IEnumerator SequenceRoutine()
    {
        // Wait for the initial 2 seconds
        yield return new WaitForSeconds(waitBeforeFade);

        // Trigger your SceneFader singleton
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeToScene(targetSceneName);
        }
        else
        {
            Debug.LogError("SceneFader Instance not found in the scene!");
            // Fallback just in case SceneFader isn't ready
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }
    }
}