using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [Header("Data")]
    public WaveData waveData;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public float clearRadius = 0.5f;

    [Header("Runtime")]
    public int currentWaveIndex;
    public bool isSpawning;
    public bool allWavesComplete;

    private Dictionary<Transform, Enemy> occupancyMap = new Dictionary<Transform, Enemy>();

    private void Start()
    {
        foreach (var pt in spawnPoints) occupancyMap[pt] = null;

        if (waveData == null) return;
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

                bool spawned = false;
                while (!spawned)
                {
                    spawned = TrySpawnEnemy(entry.enemyPrefab);
                    if (!spawned) yield return new WaitForSeconds(0.5f);
                }

                yield return new WaitForSeconds(entry.spawnInterval);
            }
        }

        isSpawning = false;
    }

    private bool TrySpawnEnemy(GameObject prefab)
    {
        if (prefab == null) return true;

        Transform spawnPoint = GetAvailableSpawnPoint();
        if (spawnPoint == null) return false;

        GameObject enemyObj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        Enemy enemyScript = enemyObj.GetComponent<Enemy>();

        occupancyMap[spawnPoint] = enemyScript;

        return true;
    }

    private Transform GetAvailableSpawnPoint()
    {
        List<Transform> pts = new List<Transform>(spawnPoints);
        for (int i = 0; i < pts.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, pts.Count);
            Transform temp = pts[i];
            pts[i] = pts[randomIndex];
            pts[randomIndex] = temp;
        }

        foreach (Transform pt in pts)
        {
           
            if (occupancyMap[pt] == null || occupancyMap[pt].isDead)
            {
                Collider2D hit = Physics2D.OverlapCircle(pt.position, clearRadius, LayerMask.GetMask("Enemy"));
                if (hit == null) return pt;
            }
        }

        return null;
    }

    private IEnumerator WaitForAllEnemiesDead()
    {
        while (true)
        {
            Enemy[] remaining = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            bool anyAlive = false;
            foreach (Enemy e in remaining)
            {
                if (!e.isDead) { anyAlive = true; break; }
            }
            if (!anyAlive) yield break;
            yield return new WaitForSeconds(0.5f);
        }
    }
}