using UnityEngine;
using DG.Tweening;

public class RuffianGrem : Gremurin
{
    [Header("Ruffian Currency Settings")]
    public float baseDropCooldown = 15f;    
    public float twinDropCooldown = 7.5f;   

    [Header("Proximity Settings")]
    public float synergyRadius = 3.0f;      

    [Header("Visual Synergy Cue")]
    public Color twinSynergyColor = new Color(1f, 0.8f, 0.9f);

    private bool hasTwinSynergy = false;
    private DropSpawner dropSpawner;

    
    public override float CurrentCurrencyOutputRate
    {
        get { return hasTwinSynergy ? twinDropCooldown : baseDropCooldown; }
    }

    protected override void Start()
    {
        base.Start();
        dropSpawner = GetComponent<DropSpawner>();

        
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

        
        CheckForTwinProximity();

        
        base.HandleWander();
    }

    private void CheckForTwinProximity()
    {
        RuffianGrem[] allRuffians = FindObjectsByType<RuffianGrem>(FindObjectsSortMode.None);

        bool foundValidPartner = false;

        foreach (var ruffian in allRuffians)
        {
            
            if (ruffian == this || ruffian.isDead || ruffian.isPickedUp) continue;

            
            float distance = Vector3.Distance(transform.position, ruffian.transform.position);
            if (distance <= synergyRadius)
            {
                foundValidPartner = true;
                break; 
            }
        }

        
        if (foundValidPartner != hasTwinSynergy)
        {
            hasTwinSynergy = foundValidPartner;
            UpdateSynergyVisuals();

            
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

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.2f); 
        Gizmos.DrawWireSphere(transform.position, synergyRadius);
    }
}