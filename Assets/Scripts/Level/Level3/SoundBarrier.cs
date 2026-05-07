using UnityEngine;
using DG.Tweening;

public class SoundBarrier : MonoBehaviour
{
    public float duration = 10f;

    void Start()
    {
        // Visual "pop in"
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

        // Auto-destroy after time
        Invoke("Expire", duration);
    }

    void Expire()
    {
        transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => Destroy(gameObject));
    }
}