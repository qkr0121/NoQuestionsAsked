using System;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySelectUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;           // 처음에 꺼 둠
    [SerializeField] private Button normalRowButton;     // 줄 전체(칸 포함)를 누를 수 있는 투명 버튼
    [SerializeField] private Button survivalRowButton;
    [SerializeField] private PaperCheckBox normalBox;
    [SerializeField] private PaperCheckBox survivalBox;
    [SerializeField] private Button signButton;
    [SerializeField] private PaperStamp stamp;
    [SerializeField] private Button backButton;

    private GameDifficulty? selected;
    private bool signing;
    private Action<GameDifficulty> onChosen;
    private Action onCancelled;

    private void Awake()
    {
        normalRowButton.onClick.AddListener(() => Select(GameDifficulty.Normal));
        survivalRowButton.onClick.AddListener(() => Select(GameDifficulty.Survival));
        signButton.onClick.AddListener(OnSignClicked);
        backButton.onClick.AddListener(() => { if (!signing) onCancelled?.Invoke(); });
    }

    public void Open(Action<GameDifficulty> chosen, Action cancelled)
    {
        onChosen = chosen;
        onCancelled = cancelled;
        selected = null;
        signing = false;
        normalBox.SetChecked(false, false);
        survivalBox.SetChecked(false, false);
        stamp.Hide();
        signButton.interactable = false;   // 난이도는 비워 두고 서명할 수 없음
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    private void Select(GameDifficulty difficulty)
    {
        if (signing) return;

        // 한 항목만 체크, 같은 칸을 다시 누르면 해제
        selected = selected == difficulty ? (GameDifficulty?)null : difficulty;
        normalBox.SetChecked(selected == GameDifficulty.Normal);
        survivalBox.SetChecked(selected == GameDifficulty.Survival);
        signButton.interactable = selected.HasValue;
    }

    private void OnSignClicked()
    {
        if (!selected.HasValue || signing) return;

        signing = true;
        signButton.interactable = false;
        GameDifficulty chosen = selected.Value;
        stamp.Play(() =>
        {
            signing = false;
            onChosen?.Invoke(chosen);
        });
    }
}
