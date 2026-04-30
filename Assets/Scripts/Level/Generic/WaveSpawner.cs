using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [Header("Data")]
    public WaveData waveData;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;

    [Header("Runtime")]
    public int currentWaveIndex;
    public bool isSpawning;
    public bool allWavesComplete;

    private void Start()
    {
        if (waveData == null)
        {
            Debug.LogError("WaveSpawner: No WaveData assigned.");
            return;
        }

        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(waveData.initialDelay);

        while (currentWaveIndex < waveData.waves.Count)
        {
            if (!LevelManager.Instance.levelActive) yield break;

            Wave wave = waveData.waves[currentWaveIndex];

            yield return StartCoroutine(SpawnWave(wave));

            if (wave.waitForClear)
                yield return StartCoroutine(WaitForAllEnemiesDead());

            currentWaveIndex++;

            if (currentWaveIndex < waveData.waves.Count)
                yield return new WaitForSeconds(waveData.waves[currentWaveIndex].delayBefore);
        }

        allWavesComplete = true;
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        isSpawning = true;

        foreach (WaveEntry entry in wave.entries)
        {
            for (int i = 0; i < entry.count; i++)
            {
                if (!LevelManager.Instance.levelActive) yield break;

                SpawnEnemy(entry.enemyPrefab);
                yield return new WaitForSeconds(entry.spawnInterval);
            }
        }

        isSpawning = false;
    }

    private IEnumerator WaitForAllEnemiesDead()
    {
        while (true)
        {
            Enemy[] remaining = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

            bool anyAlive = false;
            foreach (Enemy e in remaining)
            {
                if (!e.isDead)
                {
                    anyAlive = true;
                    break;
                }
            }

            if (!anyAlive) yield break;

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null) return;

        Transform spawnPoint = GetRandomSpawnPoint();
        Instantiate(prefab, spawnPoint.position, Quaternion.identity);
    }

    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return transform;

        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}