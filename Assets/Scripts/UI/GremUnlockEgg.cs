using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using DG.Tweening;

public class GremUnlockEgg : MonoBehaviour, IPointerClickHandler
{
    [Header("Egg References")]
    public Image eggImage;
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Grem References")]
    public Image gremImage;
    public TextMeshProUGUI gremNameText;
    public TextMeshProUGUI gremFlavorText;

    [Header("Settings")]
    public int clicksToHatch = 8;
    public float maxShakeStrength = 15f;

    private int clickCount;
    private bool hatched;
    private GremData pendingData;

    public void Init(GremData data)
    {
        pendingData = data;
        clickCount = 0;
        hatched = false;
        eggImage.sprite = closedSprite;
        eggImage.color = Color.white;
        eggImage.gameObject.SetActive(true);
        eggImage.transform.localScale = Vector3.zero;

        if (gremImage != null)
        {
            gremImage.color = new Color(1, 1, 1, 0);
            gremImage.transform.localScale = Vector3.zero;
            gremImage.gameObject.SetActive(false);
        }

        if (gremNameText != null)
        {
            gremNameText.alpha = 0;
            gremNameText.gameObject.SetActive(false);
        }
        if (gremFlavorText != null)
        {
            gremFlavorText.alpha = 0;
            gremFlavorText.gameObject.SetActive(false);
        }

        eggImage.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (hatched) return;
        clickCount++;

        float shakeStrength = Mathf.Lerp(2f, maxShakeStrength, (float)clickCount / clicksToHatch);

        eggImage.rectTransform.DOKill();
        eggImage.rectTransform.DOShakeAnchorPos(0.3f, shakeStrength, 20, 90f).SetUpdate(true);
        eggImage.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0.5f).SetUpdate(true);

        if (closedSprite != null && openSprite != null && clickCount == clicksToHatch / 2)
            eggImage.sprite = openSprite;

        if (clickCount >= clicksToHatch)
            StartCoroutine(HatchSequence());
    }

    private IEnumerator HatchSequence()
    {
        hatched = true;
        if (pendingData == null) yield break;

        if (gremImage != null)
        {
            gremImage.sprite = pendingData.sprite;
            gremImage.SetNativeSize(); 
            gremImage.gameObject.SetActive(true);
        }

        eggImage.rectTransform.DOKill();
        eggImage.rectTransform.DOShakeAnchorPos(0.5f, maxShakeStrength * 1.5f, 30, 90f).SetUpdate(true);

        yield return new WaitForSecondsRealtime(0.5f);

        if (gremImage != null)
        {
            gremImage.transform.SetAsLastSibling();
            gremImage.DOFade(1f, 0.5f).SetUpdate(true);
            gremImage.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        eggImage.DOFade(0f, 0.4f).SetUpdate(true);
        yield return new WaitForSecondsRealtime(0.6f);

        if (gremNameText != null)
        {
            gremNameText.gameObject.SetActive(true);
            gremNameText.text = pendingData.gremName;
            DOTween.To(() => gremNameText.alpha, x => gremNameText.alpha = x, 1f, 0.3f).SetUpdate(true);
        }

        yield return new WaitForSecondsRealtime(0.2f);

        if (gremFlavorText != null)
        {
            gremFlavorText.gameObject.SetActive(true); 
            gremFlavorText.text = pendingData.flavorText;
            DOTween.To(() => gremFlavorText.alpha, x => gremFlavorText.alpha = x, 1f, 0.4f).SetUpdate(true);
        }

        yield return new WaitForSecondsRealtime(0.4f);

        LevelCompleteUI.Instance?.SetButtonsInteractable(true);
    }
}