using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New CampaignDefinition", menuName = "Gremurin/CampaignDefinition")]
public class CampaignDefinition : ScriptableObject
{
    [Header("Identity")]
    public string campaignId;
    public string displayName;
    public Sprite thumbnail;

    [Header("Unlock")]
    public CampaignUnlockType unlockType;
    public string previousCampaignId;
    public int requiredTotalStars;

    [Header("Levels")]
    public List<LevelDefinition> levels;
}

public enum CampaignUnlockType
{
    AlwaysUnlocked,
    CompletePrevious,
    RequiredStars
}