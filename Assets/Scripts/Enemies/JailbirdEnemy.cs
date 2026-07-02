using UnityEngine;
using DG.Tweening;


public class JailbirdEnemy : Enemy
{
    [Header("Jailbird Unique Movement Settings")]
    public float curveFrequency = 3f;
    public float curveMagnitude = 1.2f;
    public float arrivalThreshold = 0.3f;

    private float aliveTime;

    
    protected override void Start()
    {
        
        base.Start();

        aliveTime = 0f;
    }

    
    protected override void Think()
    {
        
        FindTarget();
        MoveTowardTarget();
        TryAttack();
    }

    
    
    protected override void FindTarget()
    {
        
        base.FindTarget();
        if (targetGrem != null)
        {
            targetDrop = null; 
        }
    }

    protected override void MoveTowardTarget()
    {
        if (targetGrem == null)
        {
            UpdateMovingAnimation(false); 
            FlyOffScreen();
            return;
        }

        aliveTime += Time.deltaTime;

        
        Vector3 dirToTarget = (targetGrem.transform.position - transform.position).normalized;
        Vector3 perpendicular = new Vector3(-dirToTarget.y, dirToTarget.x, 0);

        float swoop = Mathf.Sin(aliveTime * curveFrequency) * curveMagnitude;
        Vector3 movementVector = dirToTarget + (perpendicular * swoop);

        transform.position += movementVector.normalized * moveSpeed * Time.deltaTime;

        
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

        
        if (Vector3.Distance(transform.position, targetGrem.transform.position) < arrivalThreshold)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        
        targetGrem.TakeDamage(damage);

        
        isDead = true;
        UpdateMovingAnimation(false);
        transform.DOKill();

        transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => {
            Destroy(gameObject);
        });
    }

    private void FlyOffScreen()
    {
        
        transform.position += transform.up * moveSpeed * Time.deltaTime;

        if (Mathf.Abs(transform.position.x) > 20 || Mathf.Abs(transform.position.y) > 20)
        {
            Destroy(gameObject);
        }
    }
}
