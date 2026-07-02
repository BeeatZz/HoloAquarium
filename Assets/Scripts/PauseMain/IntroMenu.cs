using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroMenu : MonoBehaviour
{
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private GameObject exitButton;
    [SerializeField] private GameObject optionsPanel;

    public void Play()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenOptions()
    {
        playButton.SetActive(false);
        optionsButton.SetActive(false);
        exitButton.SetActive(false);

        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);

        playButton.SetActive(true);
        optionsButton.SetActive(true);
        exitButton.SetActive(true);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}