using UnityEngine;
using DG.Tweening;

public class JailbirdEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 6f;
    public float curveFrequency = 3f; 
    public float curveMagnitude = 1.2f; 
    public float arrivalThreshold = 0.3f;

    [Header("Combat Settings")]
    public float damage = 25f;

    private Transform targetGrem;
    private float aliveTime;
    private Vector3 lastDirection;

 
    public void Initialize(Transform target)
    {
        targetGrem = target;
        aliveTime = 0f;
    }

    void Update()
    {
        if (targetGrem == null)
        {
            FlyOffScreen();
            return;
        }

        aliveTime += Time.deltaTime;

        Vector3 dirToTarget = (targetGrem.position - transform.position).normalized;

        Vector3 perpendicular = new Vector3(-dirToTarget.y, dirToTarget.x, 0);

        float swoop = Mathf.Sin(aliveTime * curveFrequency) * curveMagnitude;
        Vector3 movementVector = dirToTarget + (perpendicular * swoop);

        transform.position += movementVector.normalized * speed * Time.deltaTime;

        if (movementVector != Vector3.zero)
        {
            float angle = Mathf.Atan2(movementVector.y, movementVector.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        if (Vector3.Distance(transform.position, targetGrem.position) < arrivalThreshold)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        Gremurin grem = targetGrem.GetComponent<Gremurin>();
        if (grem != null)
        {
            grem.TakeDamage(damage);
        }

        transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => {
            UnityEngine.Object.Destroy(gameObject);
        });
    }

    private void FlyOffScreen()
    {
        transform.position += transform.up * speed * Time.deltaTime;

        if (Mathf.Abs(transform.position.x) > 20 || Mathf.Abs(transform.position.y) > 20)
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }
}