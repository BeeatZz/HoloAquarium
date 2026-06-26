using UnityEngine;

public class RedInkHazard : MonoBehaviour
{
    private float damageTick = 5f;
    private Gremurin grem;

    private void Start() => grem = GetComponent<Gremurin>();

    private void Update()
    {
        // If player picks them up, the Gremurin script handles isPickedUp
        if (grem.isPickedUp)
        {
            Destroy(this); // Hazard cleared!
            return;
        }

        grem.TakeDamage(damageTick * Time.deltaTime);
    }
}