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
        PendingLoad.Reset();

        RunState.Instance.ResetForNewRun();
        CurrencyManager.Instance.ResetCrystal();
        PlayerManager.Instance.ResetHealth();

        SceneTransitioner.GoTo("Base");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
