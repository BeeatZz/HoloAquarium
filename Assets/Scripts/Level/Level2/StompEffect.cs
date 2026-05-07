using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class StompEffect : MonoBehaviour
{
    [Header("Hazard Settings")]
    public float warningDuration = 2.5f;
    public float blastDuration = 0.5f;
    public float damageRadius = 1.6f;
    public float damageAmount = 35f;

    [Header("Visual References")]
    public SpriteRenderer warningCircle;
    public SpriteRenderer stompVisual;
    public SpriteRenderer shockwaveVisual;
    public float circleScaleTarget = 3.5f;
    public float pawScaleMultiplier = 0.8f;
    public int sortingOrder = 50;

    private AudioSource audioSource;
    private bool isExecuting = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (warningCircle) warningCircle.gameObject.SetActive(false);
        if (stompVisual) stompVisual.gameObject.SetActive(false);
        if (shockwaveVisual) shockwaveVisual.gameObject.SetActive(false);
    }

    public void ExecuteAttack(Vector3 pos, float specificWarningTime, AudioClip sfx)
    {
        if (isExecuting) return;
        StartCoroutine(AttackRoutine(pos, specificWarningTime, sfx));
    }

    private IEnumerator AttackRoutine(Vector3 pos, float specificWarningTime, AudioClip sfx)
    {
        isExecuting = true;

        warningCircle.gameObject.SetActive(true);
        warningCircle.sortingOrder = sortingOrder;
        warningCircle.transform.localScale = Vector3.zero;
        warningCircle.color = new Color(1f, 0.3f, 0.3f, 0f);

        if (sfx != null)
            StartCoroutine(PlayTimedAudio(Mathf.Max(0, specificWarningTime - 0.15f), sfx));

        warningCircle.transform.DOScale(Vector3.one * circleScaleTarget, 0.4f).SetUpdate(true);

        float elapsed = 0f;
        while (elapsed < specificWarningTime)
        {
            float alpha = Mathf.PingPong(elapsed * 15f, 1f);
            warningCircle.color = new Color(1f, 0.3f, 0.3f, alpha * 0.6f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        warningCircle.gameObject.SetActive(false);
        stompVisual.gameObject.SetActive(true);
        stompVisual.sortingOrder = sortingOrder + 2;
        stompVisual.transform.localScale = Vector3.one * (pawScaleMultiplier * 1.3f);
        stompVisual.transform.DOScale(Vector3.one * pawScaleMultiplier, 0.1f).SetUpdate(true);

        if (shockwaveVisual)
        {
            shockwaveVisual.gameObject.SetActive(true);
            shockwaveVisual.sortingOrder = sortingOrder + 1;
            shockwaveVisual.transform.localScale = Vector3.one * 0.2f;
            shockwaveVisual.transform.DOScale(Vector3.one * 1f, 0.3f).SetEase(Ease.OutExpo).SetUpdate(true);
            shockwaveVisual.DOFade(0, 0.3f).SetUpdate(true);
        }

        Camera.main.transform.DOShakePosition(0.2f, 0.2f).SetUpdate(true);
        ApplyAreaDamage(pos);

        yield return new WaitForSecondsRealtime(blastDuration);

        stompVisual.DOFade(0, 0.2f).SetUpdate(true).OnComplete(() => Destroy(gameObject));
    }

    private IEnumerator PlayTimedAudio(float delay, AudioClip clip)
    {
        yield return new WaitForSecondsRealtime(delay);
        audioSource.PlayOneShot(clip);
    }

    private void ApplyAreaDamage(Vector3 center)
    {
        int hitboxLayer = LayerMask.GetMask("GremPickup", "GremHitbox", "Default");
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, damageRadius, hitboxLayer);
        foreach (var hit in hits)
        {
            Gremurin grem = hit.GetComponentInParent<Gremurin>() ?? hit.GetComponent<Gremurin>();
            if (grem != null && !grem.isDead) grem.TakeDamage(damageAmount);
        }
    }
}