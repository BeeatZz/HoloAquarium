using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New LevelGoal", menuName = "Gremurin/LevelGoal")]
public class LevelGoal : ScriptableObject
{
    [Header("Purchase Settings")]
    public int totalPurchasesRequired = 1;
    public List<PurchaseTier> purchaseTiers;

    [Header("Wave Requirements")]
    public bool requireAllWavesComplete = false;
    public int requiredWaveIndex = -1;

    [Header("Time Reference")]
    public float timeLimitSeconds = 120f;
}

[System.Serializable]
public class PurchaseTier
{
    public string label;
    public float cost;
}