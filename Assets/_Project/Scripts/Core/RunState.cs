using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TimeSlot { Morning, Day, Night }

public class RunState : MonoBehaviour
{
    public static RunState Instance { get; private set; }

    [SerializeField] private int currentDay = 1;
    [SerializeField] private int missionInterval = 10;
    [SerializeField] private int missionQuota = 100;

    public int CurrentDay => currentDay;
    public TimeSlot CurrentSlot { get; private set; } = TimeSlot.Morning;

    public Mission CurrentMission { get; private set; }
    public WaveDefinition SelectedWave { get; private set; }
    public TimeOfDayModifier SelectedTimeOfDay { get; private set; }

    public event Action<Mission> OnMissionIssued;
    public event Action<bool> OnMissionResolved;

    private void Awake()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Base") return;
        if (CurrentMission != null) return;

        if (PendingLoad.ShouldLoad)
        {
            SaveData data = SaveManager.Instance.Load();
            if (data != null)
            {
                LoadFrom(data);
                PendingLoad.ShouldLoad = false;
                return;
            }
            PendingLoad.ShouldLoad = false;
        }

        IssueNewMission();
    }

    public void SelectWave(WaveDefinition wave, TimeOfDayModifier timeOfDay)
    {
        SelectedWave = wave;
        SelectedTimeOfDay = timeOfDay;
    }

    public void ClearSelectedWave()
    {
        SelectedWave = null;
        SelectedTimeOfDay = null;
    }

    public void AdvanceSlot()
    {
        if (CurrentSlot == TimeSlot.Night)
        {
            AdvanceDay();
            CurrentSlot = TimeSlot.Morning;
        }
        else
        {
            CurrentSlot = CurrentSlot + 1;
        }

        SaveManager.Instance.Save();
    }

    public void AdvanceDay()
    {
        currentDay++;

        if (CurrentMission != null && !CurrentMission.IsSubmitted && CurrentMission.IsOverdue(currentDay))
        {
            OnMissionResolved?.Invoke(false);
        }
        
        if(IsMissionIssueDay())
        {
            IssueNewMission();
        }
    }

    private bool IsMissionIssueDay()
    {
        return (currentDay - 1) % missionInterval == 0;
    }

    private void IssueNewMission()
    {
        CurrentMission = new Mission
        {
            MissionName = $"크리스탈 상납",
            RequiredCrystal = missionQuota,
            IssuedDay = CurrentDay,
            DueDay = currentDay + missionInterval - 1,
            IsSubmitted = false
        };

        OnMissionIssued?.Invoke(CurrentMission);
    }

    public bool TrySubmitMission()
    {
        if (CurrentMission == null || CurrentMission.IsSubmitted) return false;
        if (CurrencyManager.Instance.Crystal < CurrentMission.RequiredCrystal) return false;

        CurrencyManager.Instance.TrySpend(CurrentMission.RequiredCrystal);
        CurrentMission.IsSubmitted = true;

        OnMissionResolved?.Invoke(true);
        return true;
    }

    private void LoadFrom(SaveData data)
    {
        currentDay = data.currentDay;
        CurrentSlot = (TimeSlot)data.currentSlot;

        CurrencyManager.Instance.SetCrystal(data.crystal);
        if(data.player != null)
        {
            PlayerManager.Instance.SetHealth(data.player.currentHealth, data.player.maxHealth);
        }

        if(data.missions != null && data.missions.Length > 0)
        {
            MissionSaveEntry entry = data.missions[0];
            CurrentMission = new Mission
            {
                MissionName = entry.missionName,
                RequiredCrystal = entry.requiredCrystal,
                IssuedDay = entry.issuedDay,
                DueDay = entry.dueDay,
                IsSubmitted = entry.isSubmitted
            };
            OnMissionIssued?.Invoke(CurrentMission);
        }
        else
        {
            CurrentMission = null;
        }
    }

    public void ResetForNewRun()
    {
        currentDay = 1;
        CurrentSlot = TimeSlot.Morning;
        CurrentMission = null;
        SelectedWave = null;
        SelectedTimeOfDay = null;
    }
}
