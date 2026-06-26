using UnityEngine;
using DG.Tweening;

public class RuffianGrem : Gremurin
{
    [Header("Ruffian Currency Settings")]
    public float baseDropCooldown = 15f;    // Drop money every 15 seconds normally
    public float twinDropCooldown = 7.5f;   // Drop money twice as fast if a twin is close!

    [Header("Proximity Settings")]
    public float synergyRadius = 3.0f;      // How close they must be to trigger the boost

    [Header("Visual Synergy Cue")]
    public Color twinSynergyColor = new Color(1f, 0.8f, 0.9f);

    private bool hasTwinSynergy = false;
    private DropSpawner dropSpawner;

    // Overrides Gremurin base implementation to deliver current speed modifiers
    public override float CurrentCurrencyOutputRate
    {
        get { return hasTwinSynergy ? twinDropCooldown : baseDropCooldown; }
    }

    protected override void Start()
    {
        base.Start();
        dropSpawner = GetComponent<DropSpawner>();

        // Desynchronize initial spawner startup times
        if (dropSpawner != null)
        {
            dropSpawner.outputTimer = UnityEngine.Random.Range(baseDropCooldown * 0.5f, baseDropCooldown);
        }
    }

    protected override void HandleWander()
    {
        if (currentHunger < data.maxHunger * seekFoodHungerThreshold)
        {
            SeekFood();
            return;
        }

        // 1. Check if a partner is active AND close enough
        CheckForTwinProximity();

        // 2. Continue regular wandering/bobbing behavior (DropSpawner handles timing loops automatically)
        base.HandleWander();
    }

    private void CheckForTwinProximity()
    {
        RuffianGrem[] allRuffians = FindObjectsByType<RuffianGrem>(FindObjectsSortMode.None);

        bool foundValidPartner = false;

        foreach (var ruffian in allRuffians)
        {
            // Skip self, dead ones, or ones currently picked up by the player
            if (ruffian == this || ruffian.isDead || ruffian.isPickedUp) continue;

            // Check distance between this twin and the other
            float distance = Vector3.Distance(transform.position, ruffian.transform.position);
            if (distance <= synergyRadius)
            {
                foundValidPartner = true;
                break; // One nearby partner is enough to trigger the link!
            }
        }

        // Apply state change if it's different from the last frame
        if (foundValidPartner != hasTwinSynergy)
        {
            hasTwinSynergy = foundValidPartner;
            UpdateSynergyVisuals();

            // Refresh the drop spawner timer calculation to process state changes on-the-fly
            if (dropSpawner != null)
            {
                dropSpawner.ResetTimer();
            }
        }
    }

    private void UpdateSynergyVisuals()
    {
        if (sr == null) return;

        if (hasTwinSynergy)
        {
            sr.color = twinSynergyColor;

            transform.DOKill();
            transform.DOScale(Vector3.one * 1.2f, 0.15f)
                     .OnComplete(() => transform.DOScale(Vector3.one, 0.15f));
        }
        else
        {
            sr.color = Color.white;
        }
    }

    // Draws a visual guide in the editor when you select a Ruffian Grem
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.2f); // Light Cyan Circle
        Gizmos.DrawWireSphere(transform.position, synergyRadius);
    }
}