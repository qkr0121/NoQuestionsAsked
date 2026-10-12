using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TimeSlot { Morning, Day, Night }

public class RunState : MonoBehaviour
{
    public static RunState Instance { get; private set; }

    [Header("런 진행 상태")]
    [SerializeField] private int currentDay = 1;

    [Header("미션 설정")]
    [SerializeField] private int missionInterval = 7;
    [SerializeField] private int missionQuota = 400;        // 첫 주기 요구 금액
    [SerializeField] private int missionQuotaStep = 100;    // 주기마다 늘어나는 금액

    [Header("지원 동기")]
    [SerializeField] private MotiveData motiveData;

    public int CurrentDay => currentDay;
    public TimeSlot CurrentSlot { get; private set; } = TimeSlot.Morning;

    // 지금은 미션 1개만 가정. 여러 미션 동시 진행(로그라이트 랜덤 미션)으로 확장 시,
    // Mission[]/List<Mission> 으로 변경하고 발급,제출,마감판정 로직을 별도 MissionManager 로 분리 예정
    public Mission CurrentMission { get; private set; }
    public WaveDefinition SelectedWave { get; private set; }
    public TimeOfDayModifier SelectedTimeOfDay { get; private set; }

    public MotiveData Motives => motiveData;
    public int MotiveIndex { get; private set; } = -1;   // -1 = 아직 고르지 않음
    public MotiveData.Motive CurrentMotive => MotiveIndex >= 0 ? motiveData.Get(MotiveIndex) : null;

    // 지금은 구독자 없음(MissionBoardUI 가 구독하던 걸 Open()의 강제 갱신과 중복이라 제거함)
    // 다만 삭제하지 않고 남겨둠 - 미션 시각적 피드백(미션보드 위 느낌표/물음표 표시) 구현 시 이 이벤트들을 사용할 예정:
    // - OnMissionIssued : 새 미션 발급 시 느낌표 표시 트리거
    // - OnMissionResolved : 실패 피드백(성공과는 별도 구현 예정)에 사용 가능
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

    public void SelectWave(WaveDefinition wave, TimeOfDayModifier timeOfDay)
    {
        SelectedWave = wave;
        SelectedTimeOfDay = timeOfDay;
    }

    public void SelectMotive(int index)
    {
        MotiveIndex = index;
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

        if (GameManager.Instance.IsGameOver) return;   // 마감 실패로 끝난 런은 저장하지 않음
        SaveManager.Instance.Save();
    }

    public void AdvanceDay()
    {
        currentDay++;

        if (CurrentMission != null && !CurrentMission.IsSubmitted && CurrentMission.IsOverdue(currentDay))
        {
            OnMissionResolved?.Invoke(false);
            GameManager.Instance.GameOver();   // 마감을 넘기면 런 실패
            return;                            // 실패한 런에는 새 미션을 발급하지 않음
        }
        
        if(IsMissionIssueDay())
        {
            IssueNewMission();
        }
    }

    public bool TrySubmitMission()
    {
        // 미션이 없거나 이미 제출됐거나, 크리스탈이 부족하면 실패
        if (CurrentMission == null || CurrentMission.IsSubmitted) return false;
        if (!CurrencyManager.Instance.TrySpend(CurrentMission.RequiredCrystal)) return false;

        CurrentMission.IsSubmitted = true;
        OnMissionResolved?.Invoke(true);
        return true;
    }

    public void ResetForNewRun()
    {
        currentDay = 1;
        MotiveIndex = -1;
        CurrentSlot = TimeSlot.Morning;
        CurrentMission = null;
        SelectedWave = null;
        SelectedTimeOfDay = null;
    }

    public void ResetEntireRun()
    {
        ResetForNewRun();
        CurrencyManager.Instance.ResetCrystal();
        PlayerManager.Instance.ResetHealth();
        GameManager.Instance.ResetGameOver();
        PlayerWeapon.Instance.ResetWeapon();
        PlayerConsumables.Instance.ResetCounts();
        BarricadeManager.Instance.ResetAllSlots();
    }

    public void LoadFrom(SaveData data)
    {
        currentDay = data.currentDay;
        CurrentSlot = (TimeSlot)data.currentSlot;
        // 동기 목록이 바뀌어 범위를 벗어난 값은 미선택으로 처리
        MotiveIndex = (data.motiveIndex >= 0 && data.motiveIndex < motiveData.Count) ? data.motiveIndex : -1;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Base") return;
        if (CurrentMission != null) return;
        // 이미 런이 진행 중(미션이 있음)이면 재트리거 방지 - Base <-> ZWD 를 오갈 때마다 이 콜백이 또 불리는데
        // 그때마다 세이브 로드/새 미션 발급이 반복되면 안 되므로 이 가드가 필수

        if (PendingLoad.ShouldLoad)
        {
            if (SaveManager.Instance.TryLoadAndApply())
            {
                PendingLoad.Reset();
                return;
            }
            PendingLoad.Reset();
        }

        IssueNewMission();
    }

    private bool IsMissionIssueDay()
    {
        // 1일차, 11일차, 21일차... (missionInterval 배수+1일째)에만 true
        return (currentDay - 1) % missionInterval == 0;
    }

    private void IssueNewMission()
    {
        // 0부터 시작하는 주기 번호 - 발급일에서 바로 계산돼 세이브에 따로 저장할 필요 없음
        int cycle = (CurrentDay - 1) / missionInterval;

        CurrentMission = new Mission
        {
            MissionName = $"크리스탈 상납",
            RequiredCrystal = missionQuota + missionQuotaStep * cycle,
            IssuedDay = CurrentDay,
            DueDay = currentDay + missionInterval - 1,
            IsSubmitted = false
        };

        OnMissionIssued?.Invoke(CurrentMission);
    }
}
