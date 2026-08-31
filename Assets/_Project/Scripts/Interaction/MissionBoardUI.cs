using UnityEngine;
using TMPro;

public class MissionBoardUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text missionNameText;
    [SerializeField] private TMP_Text daysRemainingText;
    [SerializeField] private TMP_Text requiredCrystalText;
    [SerializeField] private UnityEngine.UI.Button submitButton;
    [SerializeField] private TMP_Text waitingText;

    private void Awake()
    {
        panel.SetActive(false);
    }

    private void Start()
    {
        RunState.Instance.OnMissionIssued += RefreshDisplay;
    }

    private void OnDisable()
    {
        if(RunState.Instance != null)
        {
            RunState.Instance.OnMissionIssued -= RefreshDisplay;
        }
    }

    public void Open()
    {
        panel.SetActive(true);
        RefreshDisplay(RunState.Instance.CurrentMission);
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    private void RefreshDisplay(Mission mission)
    {
        bool hasMission = mission != null && !mission.IsSubmitted;

        missionNameText.gameObject.SetActive(hasMission);
        daysRemainingText.gameObject.SetActive(hasMission);
        requiredCrystalText.gameObject.SetActive(hasMission);
        submitButton.gameObject.SetActive(hasMission);
        waitingText.gameObject.SetActive(!hasMission);

        if(!hasMission)
        {
            return;
        }

        missionNameText.text = mission.MissionName;
        daysRemainingText.text = $"{mission.DaysRemaining(RunState.Instance.CurrentDay)}일 남음";
        requiredCrystalText.text = $"{mission.RequiredCrystal} 크리스탈";
        submitButton.interactable = CurrencyManager.Instance.Crystal >= mission.RequiredCrystal;
    }

    public void OnSubmitClicked()
    {
        if(RunState.Instance.TrySubmitMission())
        {
            RefreshDisplay(RunState.Instance.CurrentMission);
        }
    }
}
