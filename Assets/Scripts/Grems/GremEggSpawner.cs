using System;
using UnityEngine;

public class GremEggSpawner : MonoBehaviour
{
    public static GremEggSpawner Instance { get; private set; }

    [Header("References")]
    public GameObject eggPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SpawnEgg(GremData data)
    {
        if (eggPrefab == null) return;

        Vector2 min = LevelManager.Instance.playAreaMin;
        Vector2 max = LevelManager.Instance.playAreaMax;
        Vector3 spawnPos = new Vector3(
            UnityEngine.Random.Range(min.x, max.x),
            UnityEngine.Random.Range(min.y, max.y),
            0
        );

        GameObject egg = Instantiate(eggPrefab, spawnPos, Quaternion.identity);
        GremEgg eggComponent = egg.GetComponent<GremEgg>();
        if (eggComponent != null)
            eggComponent.Init(data);
    }
}