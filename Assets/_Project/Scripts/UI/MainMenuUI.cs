using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject continueButton;
    [SerializeField] private StoryUI storyUI;
    [SerializeField] private DifficultySelectUI difficultySelectUI;
    [SerializeField] private SaveSlotSelectUI slotSelectUI;
    [SerializeField] private SettingsUI settingsUI;
    [SerializeField] private MotiveSelectUI motiveSelectUI;
    [SerializeField] private GameObject titleText;         // 선택 패널이 열려 있는 동안 숨김
    [SerializeField] private GameObject buttonContainer;

    // 새 게임 흐름(난이도 -> 빈 칸 -> 동기)에서 마지막 단계까지 들고 가는 선택값
    private GameDifficulty pendingDifficulty;
    private int pendingSlot;

    private void Start()
    {
        continueButton.SetActive(SaveManager.Instance.HasAnySave());
    }

    // 이어하기: 채워진 칸을 고르면 그 칸의 런을 시작
    public void OnContinueClicked()
    {
        Transition(() =>
        {
            SetMainMenuVisible(false);
            slotSelectUI.Open(false, OnContinueSlotChosen, BackToMainMenu);
        });
    }

    private void OnContinueSlotChosen(int slot)
    {
        SaveManager.Instance.SetActiveSlot(slot);

        // 같은 실행 중에도 저장 파일 기준으로 복원되도록 메모리 상태를 먼저 리셋함
        // (안 그러면 RunState가 "런 진행 중"으로 보고 로드를 건너뜀)
        RunState.Instance.ResetEntireRun();
        PendingLoad.MarkForLoad();
        SceneTransitioner.GoTo("Base");
    }

    public void OnSettingsClicked()
    {
        Transition(() =>
        {
            SetMainMenuVisible(false);
            settingsUI.Open(CloseSettings);
        });
    }

    private void CloseSettings()
    {
        Transition(() =>
        {
            settingsUI.Close();
            SetMainMenuVisible(true);
        });
    }

    // 새 게임: 스토리 -> 난이도 -> 빈 칸 -> 동기 -> 시작
    public void OnNewGameClicked()
    {
        Transition(() =>
        {
            SetMainMenuVisible(false);
            storyUI.Open(OnStoryFinished);
        });
    }

    private void OnStoryFinished()
    {
        Transition(() =>
        {
            storyUI.Close();
            difficultySelectUI.Open(OnDifficultyChosen, BackToMainMenu);
        });
    }

    private void OnDifficultyChosen(GameDifficulty difficulty)
    {
        pendingDifficulty = difficulty;
        Transition(() =>
        {
            difficultySelectUI.Close();
            slotSelectUI.Open(true, OnNewGameSlotChosen, BackToDifficulty);
        });
    }

    private void OnNewGameSlotChosen(int slot)
    {
        pendingSlot = slot;
        Transition(() =>
        {
            slotSelectUI.Close();
            motiveSelectUI.Open(StartNewRun, BackToSlot);
        });
    }

    private void StartNewRun(int motiveIndex)
    {
        SaveManager.Instance.SetActiveSlot(pendingSlot);   // 파일은 첫 미션이 발급된 직후 RunState가 저장함
        PendingLoad.Reset(); // 혹시 이전에 설정된 로드 플래그가 남아있을 경우를 대비해 명시적으로 초기화

        RunState.Instance.ResetEntireRun();
        // 영속 매니저들의 메모리 상태를 전부 새 런 기준을 리셋(세이브 파일 삭제와 메모리 상태 리셋은 별개)
        RunState.Instance.SelectMotive(motiveIndex);   // 리셋한 뒤에 설정해야 지워지지 않음
        RunState.Instance.SetDifficulty(pendingDifficulty);

        PendingTutorial.MarkForShow();   // 새 게임으로 기지에 들어오면 안내 패널을 자동으로 띄움
        SceneTransitioner.GoTo("Base");
    }

    private void BackToMainMenu()
    {
        Transition(() =>
        {
            difficultySelectUI.Close();
            slotSelectUI.Close();
            SetMainMenuVisible(true);
            continueButton.SetActive(SaveManager.Instance.HasAnySave());   // 칸을 지웠을 수 있음
        });
    }

    private void BackToDifficulty()
    {
        Transition(() =>
        {
            slotSelectUI.Close();
            difficultySelectUI.Open(OnDifficultyChosen, BackToMainMenu);
        });
    }

    private void BackToSlot()
    {
        Transition(() =>
        {
            motiveSelectUI.Close();
            slotSelectUI.Open(true, OnNewGameSlotChosen, BackToDifficulty);
        });
    }

    // 화면이 어두운 사이에 패널을 바꿈
    private void Transition(System.Action action)
    {
        ScreenFader.Instance.FadeAction(action);
    }

    private void SetMainMenuVisible(bool visible)
    {
        titleText.SetActive(visible);
        buttonContainer.SetActive(visible);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
