using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CrystalBeamManager : MonoBehaviour
{
    [Header("Settings")]
    public float firstBeamDelay = 8f;
    public float beamIntervalMin = 4f;
    public float beamIntervalMax = 8f;
    public int crystalsPerBeam = 3;

    [Header("References")]
    public Crystal[] crystals;

    [Header("Beam Prefab")]
    public GameObject beamHazardPrefab;

    private bool active = true;

    private void Start()
    {
        if (crystals == null || crystals.Length < 2)
        {
            Debug.LogWarning("CrystalBeamManager: Not enough crystals assigned.");
            return;
        }

        StartCoroutine(BeamLoop());
    }

    private IEnumerator BeamLoop()
    {
        yield return new WaitForSeconds(firstBeamDelay);

        while (active && LevelManager.Instance.levelActive)
        {
            FireBeam();
            float interval = UnityEngine.Random.Range(beamIntervalMin, beamIntervalMax);
            yield return new WaitForSeconds(interval);
        }
    }

    private void FireBeam()
    {
        List<Crystal> selected = SelectCrystals();
        if (selected == null || selected.Count < 2) return;

        GameObject beamObj = beamHazardPrefab != null
            ? Instantiate(beamHazardPrefab)
            : new GameObject("BeamHazard");

        BeamHazard beam = beamObj.GetComponent<BeamHazard>()
            ?? beamObj.AddComponent<BeamHazard>();

        StartCoroutine(beam.FireBeam(selected));
    }

    private List<Crystal> SelectCrystals()
    {
        if (crystals.Length < crystalsPerBeam) return null;

        List<Crystal> pool = new List<Crystal>(crystals);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Crystal temp = pool[i];
            pool[i] = pool[j];
            pool[j] = temp;
        }

        return pool.GetRange(0, crystalsPerBeam);
    }

    public void SetActive(bool state)
    {
        active = state;
    }
}