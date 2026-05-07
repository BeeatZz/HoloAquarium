using UnityEngine;
using UnityEngine.UI;

public class CampaignButton : MonoBehaviour
{
    [Header("References")]
    public LevelRegistry levelRegistry;
    public CampaignDefinition campaign;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        bool isUnlocked = SaveManager.Instance
            .IsCampaignUnlocked(campaign, levelRegistry);

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = isUnlocked ? 1f : 0.4f;
        cg.interactable = isUnlocked;
        cg.blocksRaycasts = isUnlocked;

        if (isUnlocked)
            button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        bool hasStarted = SaveManager.Instance
            .GetSaveData()
            .IsCampaignStarted(campaign.campaignId);

        if (!hasStarted)
        {
            StartCampaign(false);
        }
        else
        {
            ContinueNewGamePopup.Instance.Show(
                campaign.displayName,
                onContinueCallback: () => ContinueCampaign(),
                onNewGameCallback: () => StartCampaign(true)
            );
        }
    }

    private void StartCampaign(bool isNewGame)
    {
        if (campaign.levels == null || campaign.levels.Count == 0)
        {
            Debug.LogError($"Campaign {campaign.campaignId} has no levels.");
            return;
        }

        LevelDefinition firstLevel = campaign.levels[0];
        SaveManager.Instance.StartCampaign(campaign.campaignId, firstLevel.levelId);
        LaunchLevel(firstLevel);
    }

    private void ContinueCampaign()
    {
        string currentLevelId = SaveManager.Instance
            .GetSaveData()
            .GetCurrentLevelId(campaign.campaignId);

        LevelDefinition levelToLoad = null;

        if (!string.IsNullOrEmpty(currentLevelId))
            levelToLoad = levelRegistry.GetLevel(currentLevelId);

        if (levelToLoad == null && campaign.levels.Count > 0)
            levelToLoad = campaign.levels[0];

        if (levelToLoad == null)
        {
            Debug.LogError($"Could not find level to continue for {campaign.campaignId}");
            return;
        }

        LaunchLevel(levelToLoad);
    }

    private void LaunchLevel(LevelDefinition level)
    {
        LevelLoader.PendingLevelId = level.levelId;
        LevelLoader.PendingCampaignId = campaign.campaignId;
        SceneFader.Instance.FadeToScene(level.sceneName);
    }
}