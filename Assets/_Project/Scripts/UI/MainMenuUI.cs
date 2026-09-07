using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject continueButton;

    private void Start()
    {
        continueButton.SetActive(SaveManager.Instance.HasSave());
    }

    public void OnContinueClicked()
    {
        PendingLoad.MarkForLoad();
        SceneTransitioner.GoTo("Base");
    }

    public void OnNewGameClicked()
    {
        SaveManager.Instance.DeleteSave();
        PendingLoad.Reset(); // 혹시 이전에 설정된 로드 플래그가 남아있을 경우를 대비해 명시적으로 초기화

        RunState.Instance.ResetForNewRun();
        CurrencyManager.Instance.ResetCrystal();
        PlayerManager.Instance.ResetHealth();
        // 영속 매니저들의 메모리 상태를 전부 새 런 기준을 리셋(세이브 파일 삭제와 메모리 상태 리셋은 별개)

        SceneTransitioner.GoTo("Base");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
