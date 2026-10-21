using UnityEngine;
using TMPro;
using System;

public class ConfirmationDialog : MonoBehaviour
{
    public static ConfirmationDialog Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text messageText;

    private Action onConfirm;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(string message, Action onConfirmAction)
    {
        messageText.text = message;
        onConfirm = onConfirmAction;
        panel.SetActive(true);
    }

    public void OnYesClicked()
    {
        panel.SetActive(false);
        int soundsBefore = CurrencyManager.Instance != null ? CurrencyManager.Instance.SoundCount : 0;
        onConfirm?.Invoke();
        onConfirm = null;

        // 구매·강화는 자기 소리(구매/강화/실패)가 나므로, 그런 소리가 나지 않은 확인에만 클릭음을 냄
        bool spendSoundPlayed = CurrencyManager.Instance != null && CurrencyManager.Instance.SoundCount != soundsBefore;
        if (!spendSoundPlayed) AudioManager.PlayClick();
    }

    public void OnNoClicked()
    {
        panel.SetActive(false);
        onConfirm = null;
    }
}
