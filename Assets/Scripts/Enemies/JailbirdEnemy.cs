using UnityEngine;
using DG.Tweening;

// 1. Inherit from Enemy instead of MonoBehaviour
public class JailbirdEnemy : Enemy
{
    [Header("Jailbird Unique Movement Settings")]
    public float curveFrequency = 3f;
    public float curveMagnitude = 1.2f;
    public float arrivalThreshold = 0.3f;

    private float aliveTime;

    // Replaces your old Initialize() - utilizes the base Start logic too
    protected override void Start()
    {
        // This runs the base class scale-in animation and grabs components (Animator, SpriteRenderer)
        base.Start();

        aliveTime = 0f;
    }

    // 2. Instead of writing a new Update(), override the base class Think() method
    protected override void Think()
    {
        // We handle target searching differently here or rely on the old one
        FindTarget();
        MoveTowardTarget();
        TryAttack();
    }

    // 3. Custom target search since Jailbird might use specialized targeting logic,
    // or we can fall back to the base script's target system.
    protected override void FindTarget()
    {
        // If we don't have a Transform target yet, convert the base class Gremurin target
        base.FindTarget();
        if (targetGrem != null)
        {
            targetDrop = null; // Ensuring no target conflicts
        }
    }

    protected override void MoveTowardTarget()
    {
        if (targetGrem == null)
        {
            UpdateMovingAnimation(false); // Stop animation
            FlyOffScreen();
            return;
        }

        aliveTime += Time.deltaTime;

        // Use 'moveSpeed' from the base class instead of local 'speed'
        Vector3 dirToTarget = (targetGrem.transform.position - transform.position).normalized;
        Vector3 perpendicular = new Vector3(-dirToTarget.y, dirToTarget.x, 0);

        float swoop = Mathf.Sin(aliveTime * curveFrequency) * curveMagnitude;
        Vector3 movementVector = dirToTarget + (perpendicular * swoop);

        transform.position += movementVector.normalized * moveSpeed * Time.deltaTime;

        // Turn on the moving animation handled by the base class
        UpdateMovingAnimation(true);

        if (movementVector != Vector3.zero)
        {
            float angle = Mathf.Atan2(movementVector.y, movementVector.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    protected override void TryAttack()
    {
        if (targetGrem == null) return;

        // Checks distance to attack
        if (Vector3.Distance(transform.position, targetGrem.transform.position) < arrivalThreshold)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        // Use 'damage' from the base class
        targetGrem.TakeDamage(damage);

        // Jailbird kamikazes/destroys itself on attack
        isDead = true;
        UpdateMovingAnimation(false);
        transform.DOKill();

        transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => {
            Destroy(gameObject);
        });
    }

    private void FlyOffScreen()
    {
        // Use base class 'moveSpeed'
        transform.position += transform.up * moveSpeed * Time.deltaTime;

        if (Mathf.Abs(transform.position.x) > 20 || Mathf.Abs(transform.position.y) > 20)
        {
            Destroy(gameObject);
        }
    }
}
