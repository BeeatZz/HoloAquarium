using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

[System.Serializable]
public class SirenEvent
{
    public float triggerTime;
    public float waveSpeed = 8f;
    [HideInInspector] public bool hasFired = false;
}

public class NerissaHazardManager : MonoBehaviour
{
    [Header("Timeline Events")]
    public List<SirenEvent> sirenTimeline = new List<SirenEvent>();

    [Header("Weighted Randomness")]
    public bool allowRandomSirens = true;
    public float checkInterval = 15f;
    public float baseChance = 10f;
    public float chanceIncrease = 10f;
    private float currentChance;

    [Header("References")]
    public GameObject warningIconPrefab;
    public Transform waveVisual;
    public Color charmColor = new Color(0.7f, 0.4f, 1f, 1f);

    private float levelTimer = 0f;

    void Start()
    {
        currentChance = baseChance;
        if (allowRandomSirens) StartCoroutine(RandomMonitor());
    }

    void Update()
    {
        if (LevelManager.Instance == null || !LevelManager.Instance.levelActive) return;

        levelTimer += Time.deltaTime;

        foreach (var sEvent in sirenTimeline)
        {
            if (levelTimer >= sEvent.triggerTime && !sEvent.hasFired)
            {
                sEvent.hasFired = true;
                StartCoroutine(SirenRoutine(sEvent.waveSpeed));
            }
        }
    }

    private IEnumerator RandomMonitor()
    {
        while (LevelManager.Instance.levelActive)
        {
            yield return new WaitForSeconds(checkInterval);
            if (UnityEngine.Random.Range(0f, 100f) < currentChance)
            {
                currentChance = baseChance;
                StartCoroutine(SirenRoutine(8f));
            }
            else
            {
                currentChance += chanceIncrease;
            }
        }
    }

    private IEnumerator SirenRoutine(float speed)
    {
        // 1. Warning Phase
        for (int i = 0; i < 4; i++)
        {
            Vector3 rPos = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-3f, 3f), 0);
            GameObject icon = Instantiate(warningIconPrefab, rPos, Quaternion.identity);
            UnityEngine.Object.Destroy(icon, 1.5f);
        }
        yield return new WaitForSeconds(1.5f);

        // 2. Setup Sweep (Horizontal Example)
        waveVisual.gameObject.SetActive(true);
        Vector3 start = new Vector3(-12, 0, 0);
        Vector3 end = new Vector3(12, 0, 0);
        waveVisual.position = start;

        bool moving = true;
        waveVisual.DOMove(end, 24f / speed).SetEase(Ease.Linear).OnComplete(() => moving = false);

        // 3. Active Detection Loop
        while (moving)
        {
            Gremurin[] all = UnityEngine.Object.FindObjectsByType<Gremurin>(FindObjectsSortMode.None);
            foreach (var g in all)
            {
                // Detection logic: check if Grem is hit by the moving x-coordinate of the wave
                if (Mathf.Abs(g.transform.position.x - waveVisual.position.x) < 0.6f)
                {
                    g.BeCharmed(charmColor);
                }
            }
            yield return null;
        }
        waveVisual.gameObject.SetActive(false);
    }
}