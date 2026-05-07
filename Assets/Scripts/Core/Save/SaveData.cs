using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public List<LevelSaveData> levelProgress = new List<LevelSaveData>();
    public List<string> unlockedGrems = new List<string>();
    public List<string> unlockedAchievements = new List<string>();
    public Dictionary<string, int> stats = new Dictionary<string, int>();
    public List<CampaignSaveData> campaignProgress = new List<CampaignSaveData>();

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

    public bool IsGremUnlocked(string gremName)
    {
        return unlockedGrems.Contains(gremName);
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        return unlockedAchievements.Contains(achievementId);
    }

    public void IncrementStat(string statId, int amount = 1)
    {
        if (stats.ContainsKey(statId))
            stats[statId] += amount;
        else
            stats[statId] = amount;
    }

    public int GetStat(string statId)
    {
        return stats.ContainsKey(statId) ? stats[statId] : 0;
    }
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