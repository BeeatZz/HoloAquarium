using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New WaveData", menuName = "Gremurin/WaveData")]
public class WaveData : ScriptableObject
{
    [Header("Settings")]
    public float initialDelay = 5f;
    public List<Wave> waves;
}

[System.Serializable]
public class Wave
{
    public string label;
    public float delayBefore = 5f;
    public bool waitForClear = false;
    public List<WaveEntry> entries;
}

[System.Serializable]
public class WaveEntry
{
    public GameObject enemyPrefab;
    public int count = 3;
    public float spawnInterval = 1.5f;
}