using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("상단 좌측")]
    [SerializeField] private TMP_Text crystalText;
    [SerializeField] private TMP_Text dateText;

    [Header("상단 우측")]
    [SerializeField] private TMP_Text missionText;
    [SerializeField] private Color missionNormalColor = Color.white;
    [SerializeField] private Color missionWarningColor = new Color(1f, 0.85f, 0.25f);
    [SerializeField] private Color missionDangerColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color missionDoneColor = new Color(0.4f, 0.9f, 0.4f);

    [Header("체력바")]
    [SerializeField] private RectTransform hpFill;          // 가로 스케일로 채우는 바 (피벗 왼쪽)
    [SerializeField] private Image hpFillImage;             // 저체력 색 변경용
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Color hpNormalColor = new Color(0.35f, 0.8f, 0.35f);
    [SerializeField] private Color hpLowColor = new Color(0.9f, 0.2f, 0.2f);
    [SerializeField] private float lowHealthRatio = 0.3f;
    [SerializeField] private RectTransform hpTrail;          // 체력 바 뒤의 잔상 바 (피벗 왼쪽), 비워둬도 동작
    [SerializeField] private float trailDelay = 0.4f;        // 피해를 받은 뒤 잔상이 줄어들기 시작하기까지의 대기
    [SerializeField] private float trailSpeed = 1.5f;        // 잔상이 줄어드는 속도 (초당 비율)

    private float trailRatio = -1f;     // -1 = 아직 정하지 않음, 첫 프레임에 현재 값으로 맞춤
    private float lastRatio;
    private float lastDamageTime = -999f;

    private const int DaysPerWeek = 7;   // 미션 주기와 "일주일"을 일치시키기 위한 요일 표시용 (1일차 = 월요일)
    private const int WarningDaysLeft = 2;
    private const string MissionHeader = "<size=70%><color=#B0B0B0>미션</color></size>\n";   // 머리글만 작고 회색으로 - 본문 색은 상태에 따라 바뀜
    private static readonly string[] WeekdayNames = { "월요일", "화요일", "수요일", "목요일", "금요일", "토요일", "일요일" };

    private void Update()
    {
        RunState run = RunState.Instance;

        crystalText.text = $"크리스탈 {CurrencyManager.Format(CurrencyManager.Instance.Crystal)}";
        dateText.text = GetDateLabel(run.CurrentDay);
        UpdateMission(run);
        UpdateHealth();
    }

    private void UpdateMission(RunState run)
    {
        if (run.IsEscapeReady)
        {
            missionText.text = $"{MissionHeader}지상으로 나가세요";
            missionText.color = missionDoneColor;
            return;
        }

        Mission mission = run.CurrentMission;
        if (mission == null)
        {
            missionText.text = "";
            return;
        }

        if (mission.IsSubmitted)
        {
            // 마지막 미션이면 다음 미션 대신 귀환 안내, 아니면 다음 미션은 마감일 다음 날(다음 주 월요일)
            missionText.text = mission.DueDay >= run.TotalDays
                ? $"{MissionHeader}상납 완료. {GetDateLabel(run.TotalDays + 1)}, 남은 크리스탈과 함께 귀환할 수 있습니다."
                : $"{MissionHeader}상납 완료 다음 미션 {GetDateLabel(mission.DueDay + 1)}";
            missionText.color = missionDoneColor;
            return;
        }

        int daysLeft = mission.DaysRemaining(run.CurrentDay);
        if (daysLeft <= 0)
        {
            missionText.text = $"{MissionHeader}크리스탈 상납 {CurrencyManager.Format(mission.RequiredCrystal)} 오늘 마감!";
            missionText.color = missionDangerColor;
        }
        else
        {
            missionText.text = $"{MissionHeader}크리스탈 상납 {CurrencyManager.Format(mission.RequiredCrystal)} 마감까지 {daysLeft}일";
            missionText.color = daysLeft <= WarningDaysLeft ? missionWarningColor : missionNormalColor;
        }
    }

    private void UpdateHealth()
    {
        int current = PlayerManager.Instance.CurrentHealth;
        int max = PlayerManager.Instance.MaxHealth;
        float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

        hpFill.localScale = new Vector3(ratio, 1f, 1f);
        hpFillImage.color = ratio <= lowHealthRatio ? hpLowColor : hpNormalColor;
        hpText.text = $"{current} / {max}";

        if (hpTrail != null) UpdateTrail(ratio);
    }

    private void UpdateTrail(float ratio)
    {
        // 처음에는 현재 값에서 시작 - 불러온 체력에서 줄어드는 것처럼 보이지 않게
        if (trailRatio < 0f) trailRatio = ratio;
        else if (ratio < lastRatio) lastDamageTime = Time.unscaledTime;
        lastRatio = ratio;

        // 회복·리셋으로 늘어난 값은 바로 따라가고, 줄어든 값만 잠시 뒤 천천히 따라감 (게임오버로 timeScale 이 0이어도 따라가게 unscaled)
        if (ratio >= trailRatio) trailRatio = ratio;
        else if (Time.unscaledTime - lastDamageTime >= trailDelay)
            trailRatio = Mathf.MoveTowards(trailRatio, ratio, trailSpeed * Time.unscaledDeltaTime);

        hpTrail.localScale = new Vector3(trailRatio, 1f, 1f);
    }

    // RunState는 건드리지 않고 표시 로직에서만 주차/요일로 가공함
    private static string GetDateLabel(int day)
    {
        int week = (day - 1) / DaysPerWeek + 1;
        return $"{week}주차 {WeekdayNames[(day - 1) % DaysPerWeek]}";
    }

    public void OnReturnToMainMenuClicked()
    {
        SceneTransitioner.GoTo("MainMenu");
    }
}
