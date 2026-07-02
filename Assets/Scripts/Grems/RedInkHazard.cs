using UnityEngine;

public class RedInkHazard : MonoBehaviour
{
    private float damageTick = 5f;
    private Gremurin grem;

    private void Start() => grem = GetComponent<Gremurin>();

    private void Update()
    {
        
        if (grem.isPickedUp)
        {
            Destroy(this); 
            return;
        }

        grem.TakeDamage(damageTick * Time.deltaTime);
    }
}