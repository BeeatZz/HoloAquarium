using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using DG.Tweening;

public class LevelCompleteUI : MonoBehaviour
{
    public static LevelCompleteUI Instance { get; private set; }

    [Header("References")]
    public LevelRegistry levelRegistry;
    public string currentLevelId;

    [Header("Panel")]
    public GameObject panel;
    public TextMeshProUGUI titleText;

    [Header("Stars")]
    public GameObject[] starObjects;

    [Header("Grem Unlock")]
    public GameObject gremUnlockPanel;
    public Image gremUnlockImage;
    public TextMeshProUGUI gremUnlockNameText;
    public TextMeshProUGUI gremUnlockFlavorText;

    [Header("Buttons")]
    public Button replayButton;
    public Button menuButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        panel.SetActive(false);
        gremUnlockPanel.SetActive(false);

        replayButton.onClick.AddListener(OnReplay);
        menuButton.onClick.AddListener(OnMenu);
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(LevelLoader.PendingLevelId))
            currentLevelId = LevelLoader.PendingLevelId;

        LevelManager.Instance.OnLevelComplete += OnLevelComplete;
    }

    private void OnLevelComplete()
    {
        int stars = GetStarsFromLevelManager();
        StartCoroutine(ShowLevelComplete(stars));
    }

    private int GetStarsFromLevelManager()
    {
        StarObjective[] objectives = LevelManager.Instance
            .GetComponentsInChildren<StarObjective>();

        int stars = 0;
        foreach (StarObjective obj in objectives)
            if (obj.Evaluate()) stars++;

        return Mathf.Max(1, stars);
    }

    private IEnumerator ShowLevelComplete(int stars)
    {
        yield return new WaitForSeconds(0.5f);

        Time.timeScale = 0f;
        panel.SetActive(true);

        // Hide all stars first
        foreach (GameObject star in starObjects)
            star.SetActive(false);

        // Award stars one by one
        for (int i = 0; i < starObjects.Length; i++)
        {
            if (i < stars)
            {
                yield return new WaitForSecondsRealtime(0.4f);
                starObjects[i].SetActive(true);
                starObjects[i].transform.DOPunchScale(
                    Vector3.one * 0.5f, 0.3f, 5, 0.5f
                ).SetUpdate(true);
            }
        }

        // Save progress
        SaveManager.Instance.CompleteLevel(currentLevelId, stars);

        // Check for grem reward
        LevelDefinition levelDef = levelRegistry.GetLevel(currentLevelId);
        if (levelDef != null && levelDef.gremReward != null)
        {
            bool alreadyUnlocked = SaveManager.Instance
                .GetSaveData()
                .IsGremUnlocked(levelDef.gremReward.gremName);

            if (!alreadyUnlocked)
            {
                SaveManager.Instance.UnlockGrem(levelDef.gremReward.gremName);
                yield return new WaitForSecondsRealtime(0.5f);
                yield return StartCoroutine(ShowGremUnlock(levelDef.gremReward));
            }
        }
    }

    private IEnumerator ShowGremUnlock(GremData gremData)
    {
        gremUnlockPanel.SetActive(true);
        gremUnlockPanel.transform.localScale = Vector3.zero;
        gremUnlockPanel.transform
            .DOScale(Vector3.one, 0.4f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);

        if (gremUnlockImage != null && gremData.sprite != null)
            gremUnlockImage.sprite = gremData.sprite;

        if (gremUnlockNameText != null)
            gremUnlockNameText.text = gremData.gremName;

        if (gremUnlockFlavorText != null)
            gremUnlockFlavorText.text = gremData.flavorText;

        yield return new WaitForSecondsRealtime(0.4f);
    }

    private void OnReplay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelSelect");
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnLevelComplete -= OnLevelComplete;
    }
}