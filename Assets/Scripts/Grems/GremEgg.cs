using UnityEngine;
using System.Collections;
using DG.Tweening;

public class GremEgg : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Animation Settings")]
    public float bounceSpeed = 1.2f;
    public float bounceHeight = 0.08f;
    public float shakeAmount = 0.05f;
    public float shakeSpeed = 8f;

    private GremData pendingData;
    private bool hatched;
    private Vector3 basePosition;
    private SpriteRenderer sr;

    public void Init(GremData data)
    {
        pendingData = data;
        hatched = false;
        basePosition = transform.position;
        sr = GetComponent<SpriteRenderer>();

        if (sr != null && closedSprite != null)
            sr.sprite = closedSprite;

        // Spawn pop-in animation
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    private void Update()
    {
        if (hatched) return;
        HandleBounceAndShake();
    }

    private void HandleBounceAndShake()
    {
        // Procedural animation for "living" egg feel
        float bounce = Mathf.Abs(Mathf.Sin(Time.time * bounceSpeed)) * bounceHeight;
        float shake = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;

        transform.position = new Vector3(
            basePosition.x + shake,
            basePosition.y + bounce,
            basePosition.z
        );
    }

    public void Hatch()
    {
        if (hatched || pendingData == null) return;
        hatched = true;

        StartCoroutine(HatchSequence());
    }

    private IEnumerator HatchSequence()
    {
        // Change to open sprite
        if (sr != null && openSprite != null)
            sr.sprite = openSprite;

        transform.DOKill(); // Stop the bounce/shake

        // 1. Hop Up
        Vector3 hopPeak = basePosition + new Vector3(0, 0.4f, 0);
        transform.DOMove(hopPeak, 0.15f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(0.15f);

        // 2. Squish on impact
        transform.DOMove(basePosition, 0.15f).SetEase(Ease.InQuad);
        transform.DOScaleY(0.6f, 0.15f);
        yield return new WaitForSeconds(0.15f);

        // 3. Unsquish
        transform.DOScaleY(1f, 0.1f);
        yield return new WaitForSeconds(0.1f);

        // 4. Instantiate the specific Gremurin Prefab from ScriptableObject
        if (pendingData.speciesPrefab != null)
        {
            GameObject gremObj = Instantiate(pendingData.speciesPrefab, basePosition, Quaternion.identity);
            Gremurin grem = gremObj.GetComponent<Gremurin>();
            if (grem != null)
            {
                grem.data = pendingData;
            }
        }
        else
        {
            Debug.LogError($"No Species Prefab assigned in GremData: {pendingData.gremName}");
        }

        // 5. Egg shell fades/pops out
        transform.DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject));
    }
}