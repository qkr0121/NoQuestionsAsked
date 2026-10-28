using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryUI : MonoBehaviour
{
    [SerializeField] private StoryData data;
    [SerializeField] private GameObject panel;          // 처음에 꺼진 자식 패널 + LocksInputWhileActive
    [SerializeField] private Image backgroundImage;     // 전체 화면 그림
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text pageText;         // "1 / 6"
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonLabel;  // 마지막 장에서 "서류 작성"
    [SerializeField] private Button skipButton;

    private int pageIndex;
    private Action onFinished;

    private void Awake()
    {
        panel.SetActive(false);
        prevButton.onClick.AddListener(Prev);
        nextButton.onClick.AddListener(Next);
        skipButton.onClick.AddListener(Finish);
    }

    // 끝까지 보든 스킵하든 같은 콜백으로 이어짐 - 스킵해도 서류 작성 단계는 건너뛰지 않음
    public void Open(Action finished)
    {
        onFinished = finished;
        pageIndex = 0;
        panel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    private void Prev()
    {
        if (pageIndex <= 0) return;
        pageIndex--;
        Refresh();
    }

    private void Next()
    {
        if (pageIndex >= data.Count - 1)
        {
            Finish();
            return;
        }
        pageIndex++;
        Refresh();
    }

    private void Finish()
    {
        onFinished?.Invoke();
    }

    private void Refresh()
    {
        StoryData.Page page = data.Get(pageIndex);
        backgroundImage.sprite = page.image;
        backgroundImage.enabled = page.image != null;
        titleText.text = page.title;
        bodyText.text = page.body;
        pageText.text = $"{pageIndex + 1} / {data.Count}";
        prevButton.interactable = pageIndex > 0;
        nextButtonLabel.text = pageIndex >= data.Count - 1 ? "서류 작성" : "다음";
    }
}
