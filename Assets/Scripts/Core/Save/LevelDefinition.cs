using UnityEngine;

[CreateAssetMenu(fileName = "New LevelDefinition", menuName = "Gremurin/LevelDefinition")]
public class LevelDefinition : ScriptableObject
{
    [Header("Identity")]
    public string levelId;
    public string displayName;
    public string sceneName;
    public Sprite thumbnail;

    [Header("Reward")]
    public GremData gremReward;

    [Header("Unlock")]
    public LevelUnlockType unlockType;
    public string previousLevelId;
    public int requiredTotalStars;
}

public enum LevelUnlockType
{
    AlwaysUnlocked,
    CompletePrevious,
    RequiredStars
}