using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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

    [Header("Song Unlock")]
    public GameObject newSongUnlockPanel;
    public TextMeshProUGUI newSongUnlockTitleText; // always says "New song unlocked!"
    public TextMeshProUGUI newSongUnlockNameText;  // the track's name
    public Image newSongUnlockImage;                // track's cover art

    [Header("Shared Reveal Continue Button")]
    public Button sharedRevealButton;               // reused for claim / return
    public TextMeshProUGUI sharedRevealButtonLabel;  // labels updated dynamically

    [Header("Buttons")]
    public Button replayButton;
    public Button continueButton;
    public Button menuButton;

    private string nextLevelSceneName;
    private string nextLevelId;
    private bool sharedRevealButtonPressed;

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

        if (newSongUnlockPanel != null)
            newSongUnlockPanel.SetActive(false);

        if (newSongUnlockTitleText != null)
            newSongUnlockTitleText.text = "New song unlocked!";

        if (sharedRevealButton != null)
            sharedRevealButton.gameObject.SetActive(false);

        SetButtonsInteractable(false);

        replayButton.onClick.AddListener(OnReplay);
        continueButton.onClick.AddListener(OnContinue);
        menuButton.onClick.AddListener(OnMenu);

        if (sharedRevealButton != null)
            sharedRevealButton.onClick.AddListener(() => sharedRevealButtonPressed = true);
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(LevelLoader.PendingLevelId))
            currentLevelId = LevelLoader.PendingLevelId;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelComplete += OnLevelComplete;
            UpdateObjectivesTextList();
        }
    }

    private void UpdateObjectivesTextList(bool highlightCompleted = false)
    {
        if (starConditionsText == null || LevelManager.Instance == null) return;

        StarObjective[] objectives = LevelManager.Instance.GetComponentsInChildren<StarObjective>();
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < objectives.Length; i++)
        {
            if (objectives[i] == null) continue;

            if (highlightCompleted && objectives[i].Evaluate())
            {
                sb.AppendLine($"<color=green>{i + 1} - {objectives[i].label} (Done!)</color>");
            }
            else
            {
                sb.AppendLine($"{i + 1} - {objectives[i].label}");
            }
        }

        starConditionsText.text = sb.ToString().TrimEnd();
    }

    private void OnLevelComplete()
    {
        if (string.IsNullOrEmpty(currentLevelId))
            currentLevelId = SceneManager.GetActiveScene().name;

        int stars = EvaluateStars();
        StartCoroutine(ShowLevelComplete(stars));
    }

    private IEnumerator WaitForSharedReveal(string label)
    {
        if (sharedRevealButton == null) yield break;

        if (sharedRevealButtonLabel != null)
            sharedRevealButtonLabel.text = label;

        sharedRevealButtonPressed = false;
        sharedRevealButton.gameObject.SetActive(true);

        yield return new WaitUntil(() => sharedRevealButtonPressed);

        sharedRevealButton.gameObject.SetActive(false);
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

        StarObjective[] objectives = LevelManager.Instance.GetComponentsInChildren<StarObjective>();

        foreach (GameObject star in starObjects) if (star != null) star.SetActive(false);

        // STEP 1: Stars pop up sequentially
        for (int i = 0; i < starObjects.Length; i++)
        {
            if (i < objectives.Length && objectives[i] != null)
            {
                if (objectives[i].Evaluate() && starObjects[i] != null)
                {
                    yield return new WaitForSecondsRealtime(0.4f);

                    starObjects[i].SetActive(true);
                    starObjects[i].transform
                        .DOPunchScale(Vector3.one * 0.5f, 0.3f, 5, 0.5f)
                        .SetUpdate(true);
                }
            }
        }

        yield return new WaitForSecondsRealtime(0.3f); // Wait for animations

        UpdateObjectivesTextList(highlightCompleted: true);
        SaveManager.Instance.CompleteLevel(currentLevelId, stars);

        // Registry configuration 
        string campaignId = LevelLoader.PendingCampaignId;
        if (levelRegistry == null)
        {
            Debug.LogError("LevelCompleteUI: LevelRegistry is missing!");
        }
        else if (!string.IsNullOrEmpty(campaignId))
        {
            CampaignDefinition campaign = levelRegistry.GetCampaign(campaignId);
            if (campaign != null)
            {
                int currentIndex = campaign.levels.FindIndex(l => l != null && l.levelId == currentLevelId);

                // CHECK: If this is the last level in the campaign
                if (currentIndex == campaign.levels.Count - 1)
                {
                    nextLevelSceneName = "MainMenu";
                    nextLevelId = ""; // Exiting campaign tracker parameters
                }
                else if (currentIndex >= 0 && currentIndex < campaign.levels.Count - 1)
                {
                    LevelDefinition nextLevel = campaign.levels[currentIndex + 1];
                    if (nextLevel != null)
                    {
                        SaveManager.Instance.UpdateCampaignProgress(campaignId, nextLevel.levelId);
                        nextLevelSceneName = nextLevel.sceneName;
                        nextLevelId = nextLevel.levelId;

                        LevelLoader.PendingLevelId = nextLevelId;
                        LevelLoader.PendingCampaignId = campaignId;
                    }
                }
            }
        }

        LevelDefinition levelDef = levelRegistry != null ? levelRegistry.GetLevel(currentLevelId) : null;

        bool hasTrackReward = levelDef != null && levelDef.trackReward != null
            && !SaveManager.Instance.GetSaveData().IsTrackUnlocked(levelDef.trackReward.trackName);

        bool hasGremReward = levelDef != null && levelDef.gremReward != null
            && !SaveManager.Instance.GetSaveData().IsGremUnlocked(levelDef.gremReward.gremName);


        // --- LINEAR FLOW ---

        // 1. Stars done -> Prompt first Claim Reward
        yield return StartCoroutine(WaitForSharedReveal("Claim Reward"));

        // 2. Song Unlock Sequence
        if (hasTrackReward)
        {
            SaveManager.Instance.UnlockTrack(levelDef.trackReward.trackName);
            yield return StartCoroutine(RevealSongUnlock(levelDef.trackReward));

            // Once song animation finishes -> Show claim reward button again
            yield return StartCoroutine(WaitForSharedReveal("Claim Reward"));
            newSongUnlockPanel.SetActive(false);
        }

        // 3. Grem Unlock Sequence
        if (hasGremReward)
        {
            SaveManager.Instance.UnlockGrem(levelDef.gremReward.gremName);
            gremUnlockPanel.SetActive(true);
            gremUnlockEgg.Init(levelDef.gremReward);

            // Wait until egg hatches and flavor text animation finishes completely
            yield return StartCoroutine(WaitForFlavorTextComplete());

            // Flavor text is ready -> show claim reward button labeled "Return"
            yield return StartCoroutine(WaitForSharedReveal("Return"));
            gremUnlockPanel.SetActive(false);
        }

        // 4. Final step -> Expose the navigation setup (replay/continue/menu buttons)
        if (sharedRevealButton != null) sharedRevealButton.gameObject.SetActive(false);

        SetButtonsInteractable(true);
    }

    private IEnumerator RevealSongUnlock(TrackData track)
    {
        if (newSongUnlockPanel == null) yield break;

        if (newSongUnlockNameText != null) newSongUnlockNameText.text = track.trackName;
        if (newSongUnlockImage != null) newSongUnlockImage.sprite = track.coverArt;

        newSongUnlockPanel.SetActive(true);

        if (newSongUnlockImage != null)
            newSongUnlockImage.transform.DOPunchScale(Vector3.one * 0.4f, 0.35f, 6, 0.5f).SetUpdate(true);

        yield return new WaitForSecondsRealtime(0.15f);

        if (newSongUnlockTitleText != null)
            newSongUnlockTitleText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f).SetUpdate(true);

        yield return new WaitForSecondsRealtime(0.15f);

        if (newSongUnlockNameText != null)
            newSongUnlockNameText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f).SetUpdate(true);

        yield return new WaitForSecondsRealtime(0.3f); // let punch clean up
    }

    private IEnumerator WaitForFlavorTextComplete()
    {
        // Wait gracefully for the safe completion trigger built into the Egg component
        yield return new WaitUntil(() => gremUnlockEgg.FlavorTextDone);
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

    private void OnReplay()
    {
        Time.timeScale = 1f;
        LevelLoader.PendingLevelId = currentLevelId;
        SceneFader.Instance.FadeToScene(SceneManager.GetActiveScene().name);
    }
    private void OnContinue()
    {
        if (!string.IsNullOrEmpty(nextLevelSceneName))
        {
            Time.timeScale = 1f;
            LevelLoader.PendingLevelId = nextLevelId;
            SceneFader.Instance.FadeToScene(nextLevelSceneName);
        }
    }
    private void OnMenu() { Time.timeScale = 1f; SceneFader.Instance.FadeToScene("CampaignSelect"); }

    private void OnDestroy() { if (LevelManager.Instance != null) LevelManager.Instance.OnLevelComplete -= OnLevelComplete; }
}