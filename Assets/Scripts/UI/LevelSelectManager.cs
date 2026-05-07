using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class LevelSelectManager : MonoBehaviour
{
    [Header("References")]
    public LevelRegistry levelRegistry;
    public Transform campaignContainer;
    public GameObject campaignTemplate;
    public GameObject levelButtonTemplate;

    private void Start()
    {
        PopulateLevelSelect();
    }

    private void PopulateLevelSelect()
    {
        foreach (Transform child in campaignContainer)
            Destroy(child.gameObject);

        foreach (CampaignDefinition campaign in levelRegistry.campaigns)
        {
            bool campaignUnlocked = SaveManager.Instance
                .IsCampaignUnlocked(campaign, levelRegistry);

            GameObject campaignObj = Instantiate(campaignTemplate, campaignContainer);
            campaignObj.SetActive(true);

            TextMeshProUGUI campaignName = campaignObj
                .GetComponentInChildren<TextMeshProUGUI>();
            if (campaignName != null)
                campaignName.text = campaign.displayName;

            Transform levelContainer = campaignObj.transform.Find("LevelContainer");
            if (levelContainer == null) continue;

            foreach (LevelDefinition level in campaign.levels)
            {
                bool levelUnlocked = campaignUnlocked &&
                    SaveManager.Instance.IsLevelUnlocked(level, levelRegistry);

                GameObject levelBtn = Instantiate(levelButtonTemplate, levelContainer);
                levelBtn.SetActive(true);

                // Name
                TextMeshProUGUI[] texts = levelBtn.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (TextMeshProUGUI t in texts)
                {
                    if (t.gameObject.name == "LevelName")
                        t.text = level.displayName;
                    else if (t.gameObject.name == "StarsText")
                    {
                        int stars = SaveManager.Instance
                            .GetSaveData()
                            .GetLevelStars(level.levelId);
                        t.text = stars > 0 ? $"{stars}/3" : "";
                    }
                }

                Image thumb = levelBtn.transform
                    .Find("Thumbnail")?.GetComponent<Image>();
                if (thumb != null && level.thumbnail != null)
                    thumb.sprite = level.thumbnail;

                CanvasGroup cg = levelBtn.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = levelBtn.AddComponent<CanvasGroup>();
                cg.alpha = levelUnlocked ? 1f : 0.4f;
                cg.interactable = levelUnlocked;
                cg.blocksRaycasts = levelUnlocked;

                Button btn = levelBtn.GetComponent<Button>();
                if (btn != null && levelUnlocked)
                {
                    LevelDefinition captured = level;
                    btn.onClick.AddListener(() => LaunchLevel(captured));
                }
            }
        }
    }

    private void LaunchLevel(LevelDefinition level)
    {
        LevelLoader.PendingLevelId = level.levelId;
        SceneManager.LoadScene(level.sceneName);
    }
}