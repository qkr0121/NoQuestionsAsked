using System;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySelectUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;       // 처음에 꺼 둠, 전체 화면 + 배경 Image
    [SerializeField] private Button normalButton;
    [SerializeField] private Button survivalButton;
    [SerializeField] private Button backButton;

    private Action<GameDifficulty> onChosen;
    private Action onCancelled;

    private void Awake()
    {
        normalButton.onClick.AddListener(() => onChosen?.Invoke(GameDifficulty.Normal));
        survivalButton.onClick.AddListener(() => onChosen?.Invoke(GameDifficulty.Survival));
        backButton.onClick.AddListener(() => onCancelled?.Invoke());
    }

    public void Open(Action<GameDifficulty> chosen, Action cancelled)
    {
        onChosen = chosen;
        onCancelled = cancelled;
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
    }
}
