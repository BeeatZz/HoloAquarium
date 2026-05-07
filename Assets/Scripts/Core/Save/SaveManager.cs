using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SaveManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SaveManager");
                    _instance = go.AddComponent<SaveManager>();
                }
            }

            // CRITICAL FIX: Ensure data is loaded if accessed before Awake
            if (_instance.saveData == null)
            {
                _instance.Load();
            }
            return _instance;
        }
    }

    private SaveData saveData;
    private string savePath;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (saveData == null) Load();
    }

    private void OnApplicationQuit() => Save();

    private void OnApplicationPause(bool pause)
    {
        if (pause) Save();
    }

    public void Load()
    {
        if (string.IsNullOrEmpty(savePath))
            savePath = Path.Combine(Application.persistentDataPath, "save.json");

        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            saveData = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("Save loaded.");
        }

        // Safety: Always ensure saveData is not null
        if (saveData == null)
        {
            saveData = new SaveData();
            Debug.Log("No save found, starting fresh.");
        }
    }

    public void Save()
    {
        if (saveData == null) return;
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
    }

    public void CompleteLevel(string levelId, int stars)
    {
        if (string.IsNullOrEmpty(levelId)) return;

        LevelSaveData existing = saveData.GetLevelData(levelId);

        if (existing != null)
        {
            existing.completed = true;
            if (stars > existing.stars)
                existing.stars = stars;
        }
        else
        {
            saveData.levelProgress.Add(new LevelSaveData
            {
                levelId = levelId,
                completed = true,
                stars = stars
            });
        }

        Save();
    }

    public void StartCampaign(string campaignId, string firstLevelId)
    {
        CampaignSaveData existing = saveData.GetCampaignData(campaignId);
        if (existing != null)
        {
            existing.started = true;
            existing.currentLevelId = firstLevelId;
            existing.completed = false;
        }
        else
        {
            saveData.campaignProgress.Add(new CampaignSaveData
            {
                campaignId = campaignId,
                started = true,
                currentLevelId = firstLevelId,
                completed = false
            });
        }
        Save();
    }

    public void UpdateCampaignProgress(string campaignId, string nextLevelId)
    {
        CampaignSaveData data = saveData.GetCampaignData(campaignId);
        if (data != null)
        {
            data.currentLevelId = nextLevelId;
            Save();
        }
    }

    public void CompleteCampaign(string campaignId)
    {
        CampaignSaveData data = saveData.GetCampaignData(campaignId);
        if (data != null)
        {
            data.completed = true;
            Save();
        }
    }

    public SaveData GetSaveData() => saveData;

    public void UnlockGrem(string gremName)
    {
        if (!saveData.unlockedGrems.Contains(gremName))
        {
            saveData.unlockedGrems.Add(gremName);
            Save();
        }
    }

    public void UnlockAchievement(string achievementId)
    {
        if (!saveData.unlockedAchievements.Contains(achievementId))
        {
            saveData.unlockedAchievements.Add(achievementId);
            Save();
        }
    }

    public bool IsCampaignUnlocked(CampaignDefinition campaign, LevelRegistry registry)
    {
        switch (campaign.unlockType)
        {
            case CampaignUnlockType.AlwaysUnlocked:
                return true;
            case CampaignUnlockType.CompletePrevious:
                CampaignDefinition prev = registry.GetCampaign(campaign.previousCampaignId);
                if (prev == null) return true;
                foreach (LevelDefinition level in prev.levels)
                    if (!saveData.IsLevelCompleted(level.levelId)) return false;
                return true;
            case CampaignUnlockType.RequiredStars:
                return registry.GetTotalStars(saveData) >= campaign.requiredTotalStars;
            default:
                return false;
        }
    }

    public bool IsLevelUnlocked(LevelDefinition level, LevelRegistry registry)
    {
        switch (level.unlockType)
        {
            case LevelUnlockType.AlwaysUnlocked:
                return true;
            case LevelUnlockType.CompletePrevious:
                return saveData.IsLevelCompleted(level.previousLevelId);
            case LevelUnlockType.RequiredStars:
                return registry.GetTotalStars(saveData) >= level.requiredTotalStars;
            default:
                return false;
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);
        saveData = new SaveData();
    }
}