using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

[System.Serializable]
public class BerserkEvent
{
    public string label = "Tantrum";
    public float triggerTime;
    public int stompCount = 15;
    public float spawnInterval = 0.3f;
    public float warningTime = 1.0f;
    [HideInInspector] public bool hasFired = false;
}

public class FuwamocoHazardManager : MonoBehaviour
{
    [Header("Timeline Events")]
    public List<BerserkEvent> berserkTimeline = new List<BerserkEvent>();

    [Header("Weighted Randomness")]
    public bool allowRandomTantrums = true;
    public float checkInterval = 10f;
    [Range(0, 100)] public float baseChance = 5f;
    public float chanceIncrease = 5f;
    private float currentChance;

    [Header("Standard Stomp Settings")]
    public float minStompInterval = 10f;
    public float maxStompInterval = 15f;

    [Header("References")]
    public GameObject stompPrefab;
    public AudioClip bauBauSfx;

    private float levelTimer = 0f;
    private bool standardActive = true;

    void Start()
    {
        currentChance = baseChance;
        StartCoroutine(StandardHazardLoop());
        if (allowRandomTantrums) StartCoroutine(RandomTantrumMonitor());
    }

    void Update()
    {
        if (LevelManager.Instance == null || !LevelManager.Instance.levelActive) return;

        levelTimer += Time.deltaTime;

        foreach (var bEvent in berserkTimeline)
        {
            if (levelTimer >= bEvent.triggerTime && !bEvent.hasFired)
            {
                bEvent.hasFired = true;
                TriggerBerserk(bEvent);
            }
        }
    }

    private IEnumerator RandomTantrumMonitor()
    {
        while (LevelManager.Instance.levelActive)
        {
            yield return new WaitForSeconds(checkInterval);
            if (UnityEngine.Random.Range(0f, 100f) < currentChance)
            {
                currentChance = baseChance;
                TriggerBerserk(new BerserkEvent { label = "Random Chaos", stompCount = 12, spawnInterval = 0.4f, warningTime = 1.2f });
            }
            else
            {
                currentChance += chanceIncrease;
            }
        }
    }

    private IEnumerator StandardHazardLoop()
    {
        yield return new WaitForSeconds(8f);
        while (LevelManager.Instance.levelActive)
        {
            if (standardActive)
            {
                FireStomp(GetRandomTargetPos(), 2.5f);
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(minStompInterval, maxStompInterval));
        }
    }

    public void TriggerBerserk(BerserkEvent settings) => StartCoroutine(BerserkRoutine(settings));

    private IEnumerator BerserkRoutine(BerserkEvent settings)
    {
        standardActive = false; // Stop the random background stomps

        // Initial build up shake
        Camera.main.transform.DOShakePosition(0.5f, 0.3f).SetUpdate(true);
        yield return new WaitForSecondsRealtime(0.5f);

        for (int i = 0; i < settings.stompCount; i++)
        {
            // Pick a random location
            Vector3 spawnPos = GetRandomPlayPos();

            // Create the stomp
            GameObject obj = Instantiate(stompPrefab, spawnPos, Quaternion.identity);
            StompEffect effect = obj.GetComponent<StompEffect>();

            if (effect != null)
            {
                // FORCE the logic to start
                effect.ExecuteAttack(spawnPos, settings.warningTime, bauBauSfx);
            }

            // This wait is CRITICAL. If this is too low, it looks like a mess.
            yield return new WaitForSecondsRealtime(settings.spawnInterval);
        }

        yield return new WaitForSecondsRealtime(2f);
        standardActive = true;
    }

    private void FireStomp(Vector3 pos, float duration)
    {
        if (stompPrefab == null) return;
        GameObject obj = Instantiate(stompPrefab, pos, Quaternion.identity);
        obj.GetComponent<StompEffect>()?.ExecuteAttack(pos, duration, bauBauSfx);
    }

    private Vector3 GetRandomPlayPos()
    {
        float x = UnityEngine.Random.Range(LevelManager.Instance.playAreaMin.x, LevelManager.Instance.playAreaMax.x);
        float y = UnityEngine.Random.Range(LevelManager.Instance.playAreaMin.y, LevelManager.Instance.playAreaMax.y);
        return new Vector3(x, y, 0);
    }

    private Vector3 GetRandomTargetPos()
    {
        Gremurin[] all = UnityEngine.Object.FindObjectsByType<Gremurin>(FindObjectsSortMode.None);
        if (all.Length > 0) return all[UnityEngine.Random.Range(0, all.Length)].transform.position;
        return GetRandomPlayPos();
    }
}