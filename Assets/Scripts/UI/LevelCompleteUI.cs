using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
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
    public TextMeshProUGUI starConditionsText;

    [Header("Grem Unlock")]
    public GameObject gremUnlockPanel;
    public GremUnlockEgg gremUnlockEgg;
    public Button claimRewardButton;

    [Header("Buttons")]
    public Button replayButton;
    public Button continueButton;
    public Button menuButton;

    private string nextLevelSceneName;
    private bool claimPressed;

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
        claimRewardButton.gameObject.SetActive(false);

        SetButtonsInteractable(false);

        replayButton.onClick.AddListener(OnReplay);
        continueButton.onClick.AddListener(OnContinue);
        menuButton.onClick.AddListener(OnMenu);
        claimRewardButton.onClick.AddListener(() => claimPressed = true);
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(LevelLoader.PendingLevelId))
            currentLevelId = LevelLoader.PendingLevelId;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelComplete += OnLevelComplete;
            PopulateStarConditions();
        }
    }

    private void PopulateStarConditions()
    {
        if (starConditionsText == null || LevelManager.Instance == null) return;

        StarObjective[] objectives = LevelManager.Instance.GetComponentsInChildren<StarObjective>();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < objectives.Length; i++)
            sb.AppendLine($"{i + 1} - {objectives[i].label}");

        starConditionsText.text = sb.ToString().TrimEnd();
    }

    private void OnLevelComplete()
    {
        if (string.IsNullOrEmpty(currentLevelId))
            currentLevelId = SceneManager.GetActiveScene().name;

        int stars = EvaluateStars();
        StartCoroutine(ShowLevelComplete(stars));
    }

    private int EvaluateStars()
    {
        if (LevelManager.Instance == null) return 1;
        StarObjective[] objectives = LevelManager.Instance.GetComponentsInChildren<StarObjective>();
        int stars = 0;
        foreach (StarObjective obj in objectives)
            if (obj != null && obj.Evaluate()) stars++;

        return Mathf.Max(1, stars);
    }

    private IEnumerator ShowLevelComplete(int stars)
    {
        yield return new WaitForSeconds(0.8f);

        Time.timeScale = 0f;
        panel.SetActive(true);

        foreach (GameObject star in starObjects) if (star != null) star.SetActive(false);

        for (int i = 0; i < starObjects.Length; i++)
        {
            if (i < stars && starObjects[i] != null)
            {
                yield return new WaitForSecondsRealtime(0.5f);
                starObjects[i].SetActive(true);
                starObjects[i].transform
                    .DOPunchScale(Vector3.one * 0.5f, 0.3f, 5, 0.5f)
                    .SetUpdate(true);
            }
        }

        SaveManager.Instance.CompleteLevel(currentLevelId, stars);

        string campaignId = LevelLoader.PendingCampaignId;

        if (levelRegistry == null)
        {
            Debug.LogError("LevelCompleteUI: LevelRegistry is missing! Assign it in the Inspector.");
        }
        else if (!string.IsNullOrEmpty(campaignId))
        {
            CampaignDefinition campaign = levelRegistry.GetCampaign(campaignId);

            if (campaign != null)
            {
                int currentIndex = campaign.levels.FindIndex(l => l != null && l.levelId == currentLevelId);

                if (currentIndex >= 0 && currentIndex < campaign.levels.Count - 1)
                {
                    LevelDefinition nextLevel = campaign.levels[currentIndex + 1];
                    if (nextLevel != null)
                    {
                        SaveManager.Instance.UpdateCampaignProgress(campaignId, nextLevel.levelId);
                        nextLevelSceneName = nextLevel.sceneName;
                    }
                }
            }
        }

        if (levelRegistry != null)
        {
            LevelDefinition levelDef = levelRegistry.GetLevel(currentLevelId);
            if (levelDef != null && levelDef.gremReward != null)
            {
                bool alreadyUnlocked = SaveManager.Instance.GetSaveData().IsGremUnlocked(levelDef.gremReward.gremName);

                if (!alreadyUnlocked)
                {
                    SaveManager.Instance.UnlockGrem(levelDef.gremReward.gremName);
                    claimPressed = false;
                    claimRewardButton.gameObject.SetActive(true);

                    yield return new WaitUntil(() => claimPressed);

                    claimRewardButton.gameObject.SetActive(false);
                    yield return new WaitForSecondsRealtime(0.2f);

                    gremUnlockPanel.SetActive(true);
                    gremUnlockEgg.Init(levelDef.gremReward);
                    yield break;
                }
            }
        }

        SetButtonsInteractable(true);
    }

    public void SetButtonsInteractable(bool interactable)
    {
        if (replayButton != null) SetCanvasGroup(replayButton, interactable);
        if (menuButton != null) SetCanvasGroup(menuButton, interactable);

        if (continueButton != null)
        {
            bool showContinue = interactable && !string.IsNullOrEmpty(nextLevelSceneName);
            continueButton.gameObject.SetActive(showContinue);
            SetCanvasGroup(continueButton, interactable);
        }
    }

    private void SetCanvasGroup(Button button, bool interactable)
    {
        CanvasGroup cg = button.GetComponent<CanvasGroup>();
        if (cg == null) cg = button.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = interactable ? 1f : 0.3f;
        cg.interactable = interactable;
        cg.blocksRaycasts = interactable;
    }

    private void OnReplay() { Time.timeScale = 1f; SceneFader.Instance.FadeToScene(SceneManager.GetActiveScene().name); }
    private void OnContinue() { if (!string.IsNullOrEmpty(nextLevelSceneName)) { Time.timeScale = 1f; SceneFader.Instance.FadeToScene(nextLevelSceneName); } }
    private void OnMenu() { Time.timeScale = 1f; SceneFader.Instance.FadeToScene("CampaignSelect"); }

    private void OnDestroy() { if (LevelManager.Instance != null) LevelManager.Instance.OnLevelComplete -= OnLevelComplete; }
}