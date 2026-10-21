using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TutorialData data;
    [SerializeField] private GameObject panel;          // 처음에 꺼진 자식 패널 + LocksInputWhileActive
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text pageText;         // "1 / 6"
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonLabel;  // 마지막 장에서 "닫기"
    [SerializeField] private Button skipButton;

    private int pageIndex;
    private float previousTimeScale = 1f;

    public bool IsOpen => panel.activeSelf;   // ESC 처리에서 먼저 닫을지 판단하려고 공개

    private void Awake()
    {
        panel.SetActive(false);
        prevButton.onClick.AddListener(Prev);
        nextButton.onClick.AddListener(Next);
        skipButton.onClick.AddListener(Close);
    }

    private void Start()
    {
        // 새 게임으로 처음 기지에 들어왔을 때만 자동으로 띄움 (이어하기는 건너뜀)
        if (PendingTutorial.ShouldShow)
        {
            PendingTutorial.Reset();
            Open();
        }
    }

    public void Open()
    {
        if (GameManager.Instance.IsGameOver) return;   // 게임오버 화면의 시간 정지를 되돌리지 않도록
        if (panel.activeSelf) return;

        pageIndex = 0;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;   // 전투 중에 ? 버튼으로 열어도 좀비에게 맞지 않도록 멈춤
        AudioManager.PlayPanel();
        panel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        if (!panel.activeSelf) return;

        AudioManager.PlayPanel();
        panel.SetActive(false);
        Time.timeScale = previousTimeScale;
    }

    private void Prev()
    {
        if (pageIndex > 0)
        {
            pageIndex--;
            Refresh();
        }
    }

    private void Next()
    {
        if (pageIndex >= data.Count - 1)
        {
            Close();
            return;
        }
        pageIndex++;
        Refresh();
    }

    private void Refresh()
    {
        TutorialData.Page page = data.Get(pageIndex);
        titleText.text = page.title;
        bodyText.text = page.body;
        pageText.text = $"{pageIndex + 1} / {data.Count}";
        prevButton.interactable = pageIndex > 0;
        nextButtonLabel.text = pageIndex >= data.Count - 1 ? "닫기" : "다음";
    }
}
