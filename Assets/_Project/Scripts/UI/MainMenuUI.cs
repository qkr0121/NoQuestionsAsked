using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject continueButton;
    [SerializeField] private MotiveSelectUI motiveSelectUI;
    [SerializeField] private GameObject titleText;         // 선택 패널이 열려 있는 동안 숨김
    [SerializeField] private GameObject buttonContainer;

    private void Start()
    {
        continueButton.SetActive(SaveManager.Instance.HasSave());
    }

    public void OnContinueClicked()
    {
        // 같은 실행 중에도 저장 파일 기준으로 복원되도록 메모리 상태를 먼저 리셋함
        // (안 그러면 RunState가 "런 진행 중"으로 보고 로드를 건너뜀)
        RunState.Instance.ResetEntireRun();
        PendingLoad.MarkForLoad();
        SceneTransitioner.GoTo("Base");
    }

    public void OnNewGameClicked()
    {
        if (SaveManager.Instance.HasSave())
        {
            ConfirmationDialog.Instance.Show("저장된 진행이 삭제됩니다. 새 게임을 시작하시겠습니까?", OpenMotiveSelect);
            return;
        }
        OpenMotiveSelect();
    }

    private void OpenMotiveSelect()
    {
        // 화면이 어두운 사이에 메인 메뉴를 숨기고 선택 패널을 켬
        ScreenFader.Instance.FadeAction(() =>
        {
            SetMainMenuVisible(false);
            motiveSelectUI.Open(StartNewRun, CloseMotiveSelect);
        });
    }

    private void CloseMotiveSelect()
    {
        ScreenFader.Instance.FadeAction(() =>
        {
            motiveSelectUI.Close();
            SetMainMenuVisible(true);
        });
    }

    private void SetMainMenuVisible(bool visible)
    {
        titleText.SetActive(visible);
        buttonContainer.SetActive(visible);
    }

    private void StartNewRun(int motiveIndex)
    {
        SaveManager.Instance.DeleteSave();
        PendingLoad.Reset(); // 혹시 이전에 설정된 로드 플래그가 남아있을 경우를 대비해 명시적으로 초기화

        RunState.Instance.ResetEntireRun();
        // 영속 매니저들의 메모리 상태를 전부 새 런 기준을 리셋(세이브 파일 삭제와 메모리 상태 리셋은 별개)
        RunState.Instance.SelectMotive(motiveIndex);   // 리셋한 뒤에 설정해야 지워지지 않음

        SceneTransitioner.GoTo("Base");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
