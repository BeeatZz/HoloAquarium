using UnityEngine;
using TMPro; 
using System.Collections;

public class InitialLoader : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text loadingText; 

    [Header("Configuration")]
    [SerializeField] private string targetSceneName = "MainMenu";
    [SerializeField] private float waitBeforeFade = 2f;
    [SerializeField] private float dotAnimationSpeed = 0.5f;

    private const string BaseText = "Loading";

    private void Start()
    {
        
        StartCoroutine(AnimateLoadingText());

        
        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator AnimateLoadingText()
    {
        int dotCount = 0;

        while (true)
        {
            
            loadingText.text = BaseText + new string('.', dotCount);

            yield return new WaitForSeconds(dotAnimationSpeed);

            
            dotCount = (dotCount + 1) % 4;
        }
    }

    private IEnumerator SequenceRoutine()
    {
        
        yield return new WaitForSeconds(waitBeforeFade);

        
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeToScene(targetSceneName);
        }
        else
        {
            Debug.LogError("SceneFader Instance not found in the scene!");
            
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }
    }
}