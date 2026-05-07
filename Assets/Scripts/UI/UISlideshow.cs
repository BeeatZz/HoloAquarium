using UnityEngine;
using DG.Tweening; // Don't forget this!

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
        // If we are at the last slide, go back to 0. Otherwise, go to next.
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
        // If we are at the first slide (0), go to the last slide.
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
        // Kill any existing tweens to prevent "fighting" if the user clicks fast
        contentRect.DOKill();

        float targetX = -currentIndex * slideWidth;

        // The DOTween magic
        contentRect.DOAnchorPosX(targetX, duration).SetEase(transitionEase);
    }
}