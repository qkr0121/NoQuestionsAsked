using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BarricadeRowUI : MonoBehaviour
{
    [SerializeField] private BarricadeDirection direction;
    [SerializeField] private RectTransform durabilityFill;   // 줄 안쪽 얇은 내구도 바 (피벗 왼쪽) - 가로 스케일로 채움
    [SerializeField] private Image durabilityFillImage;      // 비율에 따라 빨강~초록으로 칠함
    [SerializeField] private TMP_Text directionText;
    [SerializeField] private TMP_Text durabilityText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Dropdown levelDropdown;
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private Button actionButton;

    private enum ActionType { None, Install, Repair }

    private BarricadeSlotInfo info;
    private ActionType currentAction = ActionType.None;
    private int currentCost;

    private void Awake()
    {
        actionButton.onClick.AddListener(OnActionClicked);
        levelDropdown.onValueChanged.AddListener(_ => RefreshActionButton());
    }

    public void Refresh()
    {
        info = BarricadeManager.Instance.GetSlotInfo(direction);

        directionText.text = direction.ToString();
        BuildLevelDropdown();
        RefreshDurabilityDisplay();
        RefreshActionButton();
    }

    private void BuildLevelDropdown()
    {
        levelDropdown.ClearOptions();

        List<string> options = new List<string>();
        int selectedIndex = 0;
        int maxLevel = BarricadeManager.Instance.MaxLevel;

        for(int level = 1; level <= maxLevel; level++)
        {
            if (!BarricadeManager.Instance.IsLevelUnlocked(level)) continue;

            if (level == info.CurrentLevel) selectedIndex = options.Count;
            options.Add($"{level}단계");
        }

        levelDropdown.AddOptions(options);
        levelDropdown.value = selectedIndex;
        levelDropdown.RefreshShownValue();
    }

    private void RefreshDurabilityDisplay()
    {
        if(info.State == BarricadeState.Empty)
        {
            durabilityText.text = "-/-";
            levelText.text = "-";
            durabilityFill.localScale = new Vector3(0f, 1f, 1f);   // 설치 안 됨: 바가 비어 있음
            return;
        }

        durabilityText.text = $"{info.CurrentDurability}/{info.MaxDurability}";
        levelText.text = $"{info.CurrentLevel}단계";

        float ratio = (float)info.CurrentDurability / info.MaxDurability;
        durabilityFill.localScale = new Vector3(ratio, 1f, 1f);
        durabilityFillImage.color = Color.Lerp(Color.red, Color.green, ratio);
    }

    private int GetSelectedLevel()
    {
        string label = levelDropdown.options[levelDropdown.value].text;
        return int.Parse(label.Replace("단계", ""));
    }

    private void RefreshActionButton()
    {
        int selectedLevel = GetSelectedLevel();

        if(info.State != BarricadeState.Empty && selectedLevel == info.CurrentLevel)
        {
            // 같은 레벨 선택 중 -> 수리 또는 완전 상태
            if (info.CurrentDurability >= info.MaxDurability)
            {
                currentAction = ActionType.None;
                actionButtonText.text = "내구도 MAX";
                actionButton.interactable = false;
            }
            else
            {
                currentAction = ActionType.Repair;
                currentCost = BarricadeManager.Instance.GetRepairCost(direction);
                actionButtonText.text = $"수리({CurrencyManager.Format(currentCost)})";
                actionButton.interactable = true;
            }
        }
        else
        {
            // 빈 슬롯이거나 다른 레벨 선택 중 -> 설치/재설치
            if(!BarricadeManager.Instance.IsLevelUnlocked(selectedLevel))
            {
                currentAction = ActionType.None;
                actionButtonText.text = "잠김";
                actionButton.interactable = false;
            }
            else
            {
                currentAction = ActionType.Install;
                currentCost = BarricadeManager.Instance.InstallCost(selectedLevel);
                actionButtonText.text = $"설치({CurrencyManager.Format(currentCost)})";
                actionButton.interactable = true;
            }
        }
    }

    private void OnActionClicked()
    {
        if(currentAction == ActionType.Repair)
        {
            ConfirmationDialog.Instance.Show($"{CurrencyManager.Format(currentCost)} 크리스탈로 수리하시겠습니까?", () =>
            {
                if(!BarricadeManager.Instance.TryRepair(direction))
                {
                    ToastMessage.Instance.Show("크리스탈이 부족합니다");
                    return;
                }
                Refresh();
            });
        }
        else if(currentAction == ActionType.Install)
        {
            int level = GetSelectedLevel();
            ConfirmationDialog.Instance.Show($"{CurrencyManager.Format(currentCost)} 크리스탈로 설치하시겠습니까?", () =>
            {
                if(!BarricadeManager.Instance.TryInstall(direction, level))
                {
                    ToastMessage.Instance.Show("크리스탈이 부족합니다");
                    return;
                }
                Refresh();
            });
        }
    }
}
