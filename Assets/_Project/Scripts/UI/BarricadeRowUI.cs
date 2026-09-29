using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BarricadeRowUI : MonoBehaviour
{
    [SerializeField] private BarricadeDirection direction;
    [SerializeField] private Image rowBackground;
    [SerializeField] private TMP_Text directionText;
    [SerializeField] private TMP_Text durabilityText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Dropdown levelDropdown;
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private Button actionButton;

    private BarricadeSlotInfo info;

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
            rowBackground.color = Color.gray;
            return;
        }

        durabilityText.text = $"{info.CurrentDurability}/{info.MaxDurability}";
        levelText.text = $"{info.CurrentLevel}단계";

        float ratio = (float)info.CurrentDurability / info.MaxDurability;
        rowBackground.color = Color.Lerp(Color.red, Color.green, ratio);
    }

    private int GetSelectedLevel()
    {
        string label = levelDropdown.options[levelDropdown.value].text;
        return int.Parse(label.Replace("단계", ""));
    }

    private void RefreshActionButton()
    {

    }

    private void OnActionClicked()
    {

    }
}
