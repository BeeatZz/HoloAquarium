using UnityEngine;
using DG.Tweening;

public class PageSuckPage : MonoBehaviour
{
    private Transform target;
    private BookWyrmBoss boss;
    private float speed;
    private bool collected;

    public void Init(Transform bossTransform, BookWyrmBoss bossRef, float pageSpeed)
    {
        target = bossTransform;
        boss = bossRef;
        speed = pageSpeed;

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    private void Update()
    {
        if (collected || target == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.3f)
            ReachBoss();
    }

    private void ReachBoss()
    {
        collected = true;
        boss.HealFromPage();

        transform.DOKill();
        transform.DOScale(Vector3.zero, 0.2f)
            .OnComplete(() => Destroy(gameObject));
    }

    public void OnMouseDown()
    {
        if (collected) return;
        collected = true;
        boss.RegisterPageDestroyed();

        transform.DOKill();
        transform.DOScale(Vector3.zero, 0.2f)
            .OnComplete(() => Destroy(gameObject));
    }
}