using UnityEngine;

public class BookWyrmProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float damage;
    private float maxRange = 15f;
    private Vector3 startPos;

    public void Init(Vector3 dir, float spd, float dmg)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        startPos = transform.position;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, startPos) >= maxRange)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Gremurin grem = other.GetComponentInParent<Gremurin>();
        if (grem != null && !grem.isDead)
        {
            grem.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}