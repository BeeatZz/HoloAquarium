using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;

public class GremEggSpawner : MonoBehaviour
{
    public static GremEggSpawner Instance { get; private set; }

    [Header("References")]
    public GameObject eggPrefab;
    public GameObject gremPrefab;

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

        // Random position inside play area
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
            eggComponent.Init(data, gremPrefab);
    }
}