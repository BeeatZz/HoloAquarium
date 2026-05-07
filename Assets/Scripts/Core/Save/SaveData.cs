using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public List<LevelSaveData> levelProgress = new List<LevelSaveData>();
    public List<string> unlockedGrems = new List<string>();
    public List<string> unlockedAchievements = new List<string>();
    public List<CampaignSaveData> campaignProgress = new List<CampaignSaveData>();

    public List<StatEntry> stats = new List<StatEntry>();

    #region Campaign Helpers
    public CampaignSaveData GetCampaignData(string campaignId)
    {
        return campaignProgress.Find(c => c.campaignId == campaignId);
    }

    public bool IsCampaignStarted(string campaignId)
    {
        CampaignSaveData data = GetCampaignData(campaignId);
        return data != null && data.started;
    }

    public bool IsCampaignCompleted(string campaignId)
    {
        CampaignSaveData data = GetCampaignData(campaignId);
        return data != null && data.completed;
    }

    public string GetCurrentLevelId(string campaignId)
    {
        CampaignSaveData data = GetCampaignData(campaignId);
        return data?.currentLevelId;
    }
    #endregion

    #region Level Helpers
    public LevelSaveData GetLevelData(string levelId)
    {
        return levelProgress.Find(l => l.levelId == levelId);
    }

    public bool IsLevelCompleted(string levelId)
    {
        LevelSaveData data = GetLevelData(levelId);
        return data != null && data.completed;
    }

    public int GetLevelStars(string levelId)
    {
        LevelSaveData data = GetLevelData(levelId);
        return data?.stars ?? 0;
    }
    #endregion

    #region Unlock Helpers
    public bool IsGremUnlocked(string gremName)
    {
        return unlockedGrems.Contains(gremName);
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        return unlockedAchievements.Contains(achievementId);
    }
    #endregion

    #region Stat Helpers (Fixed for Unity Serialization)
    public void IncrementStat(string statId, int amount = 1)
    {
        StatEntry entry = stats.Find(s => s.key == statId);
        if (entry != null)
            entry.value += amount;
        else
            stats.Add(new StatEntry { key = statId, value = amount });
    }

    public int GetStat(string statId)
    {
        StatEntry entry = stats.Find(s => s.key == statId);
        return entry != null ? entry.value : 0;
    }
    #endregion
}

[Serializable]
public class StatEntry
{
    public string key;
    public int value;
}

[Serializable]
public class CampaignSaveData
{
    public string campaignId;
    public bool started;
    public bool completed;
    public string currentLevelId;
}

[Serializable]
public class LevelSaveData
{
    public string levelId;
    public bool completed;
    public int stars;
}