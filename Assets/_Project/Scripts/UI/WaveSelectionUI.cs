using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveSelectionUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject panel;

    [Header("데이터")]
    [SerializeField] private WaveDefinition[] availableWaves;
    [SerializeField] private TimeOfDayModifier[] timeOfDayOptions;

    [Header("웨이브 버튼 목록")]
    [SerializeField] private Transform waveButtonContainer;
    [SerializeField] private GameObject waveButtonPrefab;

    [Header("웨이브 정보 표시")]
    [SerializeField] private TMP_Text valueEnemyCount;
    [SerializeField] private TMP_Text valueDirections;
    [SerializeField] private TMP_Text valueStrength;
    [SerializeField] private TMP_Text valueRareChance;

    [Header("시간대 아이콘")]
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

    public void Confirm()
    {
        if (selectedWave == null || selectedTimeOfDay == null) return;

        RunState.Instance.SelectWave(selectedWave, selectedTimeOfDay);
        Close();
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
        valueEnemyCount.text = BuildEnemyCountText(wave, spawnMultiplier);

        float rareMultiplier = selectedTimeOfDay != null ?
            selectedTimeOfDay.RareCrystalDropRateMultiplier : 1f;
        int rareChancePercent = Mathf.RoundToInt(wave.RareCrystalDropChance * rareMultiplier * 100);

        valueDirections.text = wave.SpawnDirections == SpawnDirectionPattern.Four ? "4방향" : "8방향";
        valueStrength.text = GetStrengthLabel();
        valueRareChance.text = $"{rareChancePercent}%";
    }

    private string BuildEnemyCountText(WaveDefinition wave, float spawnMultiplier)
    {
        Dictionary<EnemyType, int> countByType = new Dictionary<EnemyType, int>();
        foreach (WaveDefinition.EnemySpawnSegment segment in wave.SpawnSegments)
        {
            int scaledCount = Mathf.Max(1, Mathf.RoundToInt(segment.count * spawnMultiplier));
            if (!countByType.ContainsKey(segment.type)) countByType[segment.type] = 0;
            countByType[segment.type] += scaledCount;
        }

        List<string> parts = new List<string>();
        foreach (KeyValuePair<EnemyType, int> pair in countByType)
        {
            parts.Add($"{GetEnemyTypeLabel(pair.Key)} {pair.Value}");
        }
        return string.Join(" / ", parts);
    }

    private string GetEnemyTypeLabel(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Heavy: return "헤비";
            case EnemyType.Normal: return "일반";
            default: return type.ToString();
        }
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
}
