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
        onConfirm?.Invoke();
        onConfirm = null;
    }

    public void OnNoClicked()
    {
        panel.SetActive(false);
        onConfirm = null;
    }
}
