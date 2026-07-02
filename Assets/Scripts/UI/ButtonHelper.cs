using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHelper : MonoBehaviour
{
    public LevelCompleteUI levelCompleteUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveToScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneFader.Instance.FadeToScene(sceneName);
    }
    public void OnReplay()
    {
        Time.timeScale = 1f;
        LevelLoader.PendingLevelId = levelCompleteUI.currentLevelId;
        SceneFader.Instance.FadeToScene(SceneManager.GetActiveScene().name);
    }
}
