using UnityEngine;
using DG.Tweening; 

public class UISlideshow : MonoBehaviour
{
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private float slideWidth = 800f;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Ease transitionEase = Ease.OutQuad;

    private int currentIndex = 0;
    private int totalSlides;

    void Start()
    {
        totalSlides = transform.childCount;
    }

    public void NextSlide()
    {
        if (currentIndex >= totalSlides - 1)
        {
            currentIndex = 0;
        }
        else
        {
            currentIndex++;
        }
        MoveToCurrentIndex();
    }

    public void PreviousSlide()
    {
        if (currentIndex <= 0)
        {
            currentIndex = totalSlides - 1;
        }
        else
        {
            currentIndex--;
        }
        MoveToCurrentIndex();
    }

    private void MoveToCurrentIndex()
    {
        contentRect.DOKill();

        float targetX = -currentIndex * slideWidth;

        contentRect.DOAnchorPosX(targetX, duration).SetEase(transitionEase);
    }
}