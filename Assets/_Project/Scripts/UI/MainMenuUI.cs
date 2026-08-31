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
        PendingLoad.ShouldLoad = true;
        SceneTransitioner.GoTo("Base");
    }

    public void OnNewGameClicked()
    {
        SaveManager.Instance.DeleteSave();
        PendingLoad.ShouldLoad = false;

        RunState.Instance.ResetForNewRun();
        CurrencyManager.Instance.ResetCrystal();
        PlayerManager.Instance.SetHealth(PlayerManager.Instance.MaxHealth, PlayerManager.Instance.MaxHealth);

        SceneTransitioner.GoTo("Base");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
