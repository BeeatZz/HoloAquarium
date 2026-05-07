using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ShioriHazardManager : MonoBehaviour
{
    public static ShioriHazardManager Instance;

    [Header("Prefabs")]
    public List<GameObject> fallingObjectPrefabs;

    [Header("Speed Settings")]
    public float standardInterval = 2.0f;
    public float tantrumInterval = 0.5f;  

    private float currentInterval;
    private bool hazardsActive = false;

    void Awake() => Instance = this;

    public void StartStandardAttacks()
    {
        if (hazardsActive) return;

        hazardsActive = true;
        currentInterval = standardInterval;
        StartCoroutine(HazardLoop());
    }

    public void EnterTantrum()
    {
        Debug.Log("SHIORI IS BERZERK!");
        currentInterval = tantrumInterval; 
    }

    private IEnumerator HazardLoop()
    {
        while (hazardsActive)
        {
            Vector3 targetPos = GetRandomTargetPos();
            GameObject prefab = fallingObjectPrefabs[UnityEngine.Random.Range(0, fallingObjectPrefabs.Count)];

            GameObject instance = Instantiate(prefab, targetPos, Quaternion.identity);

            if (instance.TryGetComponent(out FallingHazard hazard))
            {
                hazard.Setup(targetPos, UnityEngine.Random.Range(0.8f, 1.4f));
            }

            yield return new WaitForSeconds(currentInterval);
        }
    }

    private Vector3 GetRandomTargetPos()
    {
        if (UnityEngine.Random.value < 0.3f)
        {
            Gremurin[] all = UnityEngine.Object.FindObjectsByType<Gremurin>(FindObjectsSortMode.None);
            if (all.Length > 0) return all[UnityEngine.Random.Range(0, all.Length)].transform.position;
        }

        float x = UnityEngine.Random.Range(LevelManager.Instance.playAreaMin.x, LevelManager.Instance.playAreaMax.x);
        float y = UnityEngine.Random.Range(LevelManager.Instance.playAreaMin.y, LevelManager.Instance.playAreaMax.y);
        return new Vector3(x, y, 0);
    }
}