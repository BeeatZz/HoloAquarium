using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ContinueNewGamePopup : MonoBehaviour
{
    public static ContinueNewGamePopup Instance { get; private set; }

    [Header("References")]
    public GameObject panel;
    public TextMeshProUGUI campaignNameText;
    public Button continueButton;
    public Button newGameButton;
    public Button closeButton;

    private Action onContinue;
    private Action onNewGame;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        continueButton.onClick.AddListener(OnContinueClicked);
        newGameButton.onClick.AddListener(OnNewGameClicked);
        closeButton?.onClick.AddListener(Hide);

        Hide();
    }

    public void Show(string campaignName, Action onContinueCallback, Action onNewGameCallback)
    {
        onContinue = onContinueCallback;
        onNewGame = onNewGameCallback;

        if (campaignNameText != null)
            campaignNameText.text = campaignName;

        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
        onContinue = null;
        onNewGame = null;
    }

    private void OnContinueClicked()
    {
        Action callback = onContinue;
        Hide();
        callback?.Invoke();
    }

    private void OnNewGameClicked()
    {
        Action callback = onNewGame;
        Hide();
        callback?.Invoke();
    }
}