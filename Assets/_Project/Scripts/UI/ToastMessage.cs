using UnityEngine;
using TMPro;
using System.Collections;

public class ToastMessage : MonoBehaviour
{
    public static ToastMessage Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float displayDuration = 1.5f;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(string message)
    {
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);

        messageText.text = message;
        panel.SetActive(true);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        panel.SetActive(false);
        hideCoroutine = null;
    }
}
