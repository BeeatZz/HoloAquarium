using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System;

public class HubManager : MonoBehaviour
{
    [Header("References")]
    public GameObject hubGremPrefab;
    public List<GremData> allGremTypes;
    public GremData basicGremData; 
    public Transform spawnAreaCenter;

    [Header("Settings")]
    public float spawnRadius = 6f;
    public float spawnYOffset = 0.5f;

    private void Start()
    {
        SpawnInitialGrems();
    }

    private void SpawnInitialGrems()
    {
        if (basicGremData != null)
        {
            SpawnSpecificGrem(basicGremData);
        }

        if (SaveManager.Instance == null) return;

        List<string> unlocked = SaveManager.Instance.GetSaveData().unlockedGrems;

        foreach (GremData data in allGremTypes)
        {
            if (data == basicGremData || !unlocked.Contains(data.gremName)) continue;

            SpawnSpecificGrem(data);
        }
    }

   
    public void SpawnSpecificGrem(GremData targetGremData)
    {
        if (targetGremData == null || hubGremPrefab == null) return;

        Vector3 spawnPos = GetRandomNavMeshPoint();

        spawnPos.y += spawnYOffset;

        GameObject gremObj = Instantiate(hubGremPrefab, spawnPos, Quaternion.identity);
        HubGremurin hubGrem = gremObj.GetComponent<HubGremurin>();

        if (hubGrem != null)
        {
            hubGrem.data = targetGremData;
        }
    }

    private Vector3 GetRandomNavMeshPoint()
    {
        Vector3 center = spawnAreaCenter != null ? spawnAreaCenter.position : transform.position;
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * spawnRadius + center;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas))
            return hit.position;

        return center;
    }
}