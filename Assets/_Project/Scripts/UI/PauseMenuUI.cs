using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;        // 처음에 꺼진 자식 패널 + LocksInputWhileActive
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TutorialUI tutorialUI;
    [SerializeField] private SettingsUI settingsUI;

    private float previousTimeScale = 1f;

    private void Awake()
    {
        panel.SetActive(false);
        resumeButton.onClick.AddListener(Close);
        helpButton.onClick.AddListener(tutorialUI.Open);   // 게임 설명은 일시정지 중에 열려서 닫아도 시간 정지가 유지됨
        settingsButton.onClick.AddListener(() => settingsUI.Open(settingsUI.Close));
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;

        // 가장 위에 열린 것부터 하나씩 닫음: 설정 -> 게임 설명 -> 일시정지 메뉴, 아무것도 없으면 일시정지 메뉴를 엶
        if (settingsUI.IsOpen) settingsUI.Close();
        else if (tutorialUI.IsOpen) tutorialUI.Close();
        else if (panel.activeSelf) Close();
        else Open();
    }

    public void Open()
    {
        if (panel.activeSelf) return;
        if (GameManager.Instance.IsGameOver) return;   // 게임오버 화면의 시간 정지를 되돌리지 않도록
        if (PlayerInputLock.IsLocked) return;          // 상점·튜토리얼 등 다른 패널이 열려 있으면 열지 않음

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        panel.SetActive(true);
    }

    public void Close()
    {
        if (!panel.activeSelf) return;

        panel.SetActive(false);
        Time.timeScale = previousTimeScale;
    }

    private void OnMainMenuClicked()
    {
        ConfirmationDialog.Instance.Show("메인 메뉴로 나가시겠습니까?\n마지막 저장 이후의 진행은 사라집니다.", GoToMainMenu);
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;   // 로딩 화면이 Time.deltaTime 을 쓰므로 멈춘 시간을 되돌려야 함
        PendingRunReset.MarkForReset();
        SceneTransitioner.GoTo("MainMenu");
    }
}
