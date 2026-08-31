using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private WaveDefinition[] availableWaves;
    [SerializeField] private TimeOfDayModifier[] timeOfDayOptions;

    [SerializeField] private Transform waveButtonContainer;
    [SerializeField] private GameObject waveButtonPrefab;

    [SerializeField] private TMP_Text valueEnemyCount;
    [SerializeField] private TMP_Text valueDirections;
    [SerializeField] private TMP_Text valueStrength;
    [SerializeField] private TMP_Text valueRareChance;

    [SerializeField] private Image timeOfDayIcon;
    [SerializeField] private Sprite morningIcon;
    [SerializeField] private Sprite dayIcon;
    [SerializeField] private Sprite nightIcon;

    private WaveDefinition selectedWave;
    private TimeOfDayModifier selectedTimeOfDay;

    private void Awake()
    {
        panel.SetActive(false);
    }

    public void Open()
    {
        panel.SetActive(true);
        selectedTimeOfDay = GetTimeOfDayForCurrentSlot();
        UpdateTimeOfDayIcon();
        BuildWaveButtons();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    private TimeOfDayModifier GetTimeOfDayForCurrentSlot()
    {
        int index = (int)RunState.Instance.CurrentSlot;
        return index < timeOfDayOptions.Length ? timeOfDayOptions[index] : timeOfDayOptions[0];
    }

    private void BuildWaveButtons()
    {
        foreach(Transform child in waveButtonContainer)
        {
            Destroy(child.gameObject);
        }

        foreach(WaveDefinition wave in availableWaves)
        {
            GameObject buttonObj = Instantiate(waveButtonPrefab, waveButtonContainer);
            TMP_Text label = buttonObj.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = wave.WaveName;

            UnityEngine.UI.Button button = buttonObj.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() => ShowWaveInfo(wave));
        }
    }

    private void ShowWaveInfo(WaveDefinition wave)
    {
        selectedWave = wave;

        float spawnMultiplier = selectedTimeOfDay != null ? selectedTimeOfDay.SpawnCountMultiplier : 1f;
        int scaledEnemyCount = Mathf.Max(1, Mathf.RoundToInt(wave.TotalEnemyCount * spawnMultiplier));
        float rareMultiplier = selectedTimeOfDay != null ?
            selectedTimeOfDay.RareCrystalDropRateMultiplier : 1f;
        int rareChancePercent = Mathf.RoundToInt(wave.RareCrystalDropChance * rareMultiplier * 100);

        valueEnemyCount.text = scaledEnemyCount.ToString();
        valueDirections.text = wave.SpawnDirections == SpawnDirectionPattern.Four ? "4방향" : "8방향";
        valueStrength.text = GetStrengthLabel();
        valueRareChance.text = $"{rareChancePercent}%";
    }

    private string GetStrengthLabel()
    {
        if (selectedTimeOfDay == null) return "-";

        return selectedTimeOfDay.GetStrengthTier() switch
        {
            StrengthTier.Weak => "약함",
            StrengthTier.Strong => "강함",
            _ => "보통"
        };
    }

    private void UpdateTimeOfDayIcon()
    {
        timeOfDayIcon.sprite = RunState.Instance.CurrentSlot switch
        {
            TimeSlot.Morning => morningIcon,
            TimeSlot.Day => dayIcon,
            TimeSlot.Night => nightIcon,
            _ => dayIcon
        };
    }

    public void Confirm()
    {
        if (selectedWave == null || selectedTimeOfDay == null) return;

        RunState.Instance.SelectWave(selectedWave, selectedTimeOfDay);
        Close();
    }
}
