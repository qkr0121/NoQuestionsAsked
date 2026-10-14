using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveSelectionUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject panel;

    [Header("데이터")]
    [SerializeField] private WaveDefinition[] availableWaves;

    [Header("웨이브 버튼 목록")]
    [SerializeField] private Transform waveButtonContainer;
    [SerializeField] private GameObject waveButtonPrefab;

    [Header("웨이브 정보 표시")]
    [SerializeField] private TMP_Text valueEnemyCount;
    [SerializeField] private TMP_Text valueDirections;

    private WaveDefinition selectedWave;

    private void Awake()
    {
        panel.SetActive(false);
    }

    public void Open()
    {
        panel.SetActive(true);
        BuildWaveButtons();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    public void Confirm()
    {
        if (selectedWave == null) return;

        RunState.Instance.SelectWave(selectedWave);
        Close();
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

        valueEnemyCount.text = BuildEnemyCountText(wave);
        valueDirections.text = wave.SpawnDirections == SpawnDirectionPattern.Four ? "4방향" : "8방향";
    }

    private string BuildEnemyCountText(WaveDefinition wave)
    {
        Dictionary<EnemyType, int> countByType = new Dictionary<EnemyType, int>();
        foreach (WaveDefinition.EnemySpawnSegment segment in wave.SpawnSegments)
        {
            if (!countByType.ContainsKey(segment.type)) countByType[segment.type] = 0;
            countByType[segment.type] += segment.count;
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
}
