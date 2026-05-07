using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelRegistry", menuName = "Gremurin/LevelRegistry")]
public class LevelRegistry : ScriptableObject
{
    public List<CampaignDefinition> campaigns;

    public LevelDefinition GetLevel(string levelId)
    {
        foreach (CampaignDefinition campaign in campaigns)
        {
            foreach (LevelDefinition level in campaign.levels)
            {
                if (level.levelId == levelId)
                    return level;
            }
        }
        return null;
    }

    public CampaignDefinition GetCampaign(string campaignId)
    {
        foreach (CampaignDefinition campaign in campaigns)
        {
            if (campaign.campaignId == campaignId)
                return campaign;
        }
        return null;
    }

    public int GetTotalStars(SaveData saveData)
    {
        int total = 0;
        foreach (LevelSaveData levelData in saveData.levelProgress)
            total += levelData.stars;
        return total;
    }
}