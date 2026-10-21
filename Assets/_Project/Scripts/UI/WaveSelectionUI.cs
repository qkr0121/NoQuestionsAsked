using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private TMP_Text infoComposition;   // "적 수\n선발: ...\n돌격: ..."
    [SerializeField] private TMP_Text infoDirections;    // "스폰 방향: 2방향 (북·동)"
    [SerializeField] private TMP_Text infoStatus;        // "해금됨" / "잠김 - 다음 주 해금" / "잠김 - 이전 웨이브 클리어 필요"
    [SerializeField] private Button confirmButton;

    private static readonly string[] DirectionNames = { "북", "동", "남", "서" };   // BarricadeDirection 순서(North, East, South, West)와 일치
    private static readonly Color LockedColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private WaveDefinition selectedWave;

    private void Awake()
    {
        panel.SetActive(false);
    }

    public void Open()
    {
        AudioManager.PlayPanel();
        panel.SetActive(true);
        BuildWaveButtons();
        ShowWaveInfo(GetHighestUnlockedWave());   // 열자마자 정보 카드가 비어 있지 않게
    }

    public void Close()
    {
        if (panel.activeSelf) AudioManager.PlayPanel();
        panel.SetActive(false);
    }

    public void Confirm()
    {
        if (selectedWave == null || !RunState.Instance.IsWaveUnlocked(selectedWave)) return;

        RunState.Instance.SelectWave(selectedWave);
        Close();
    }

    private WaveDefinition GetHighestUnlockedWave()
    {
        WaveDefinition best = null;
        foreach (WaveDefinition wave in availableWaves)
        {
            if (!RunState.Instance.IsWaveUnlocked(wave)) continue;
            if (best == null || wave.WaveNumber > best.WaveNumber) best = wave;
        }
        return best != null ? best : (availableWaves.Length > 0 ? availableWaves[0] : null);
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

            // 잠긴 웨이브도 눌러서 정보는 볼 수 있고 확인 버튼만 막음 (잠금 이유를 확인할 수 있도록)
            Image image = buttonObj.GetComponent<Image>();
            if (image != null) image.color = RunState.Instance.IsWaveUnlocked(wave) ? Color.white : LockedColor;

            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => ShowWaveInfo(wave));
        }
    }

    private void ShowWaveInfo(WaveDefinition wave)
    {
        selectedWave = wave;
        if (wave == null) return;

        if (infoComposition != null) infoComposition.text = "적 수\n" + BuildCompositionText(wave);
        if (infoDirections != null) infoDirections.text = "스폰 방향: " + BuildDirectionText(wave);

        RunState run = RunState.Instance;
        bool unlocked = run.IsWaveUnlocked(wave);
        if (infoStatus != null)
        {
            infoStatus.text = unlocked ? "해금됨"
                : wave.WaveNumber > run.WaveWeekCap ? $"잠김 - {run.GetWaveUnlockWeek(wave.WaveNumber)}주차 해금"
                : "잠김 - 이전 웨이브 클리어 필요";
        }
        if (confirmButton != null) confirmButton.interactable = unlocked;
    }

    // 대기/경고가 있는 첫 구간부터가 돌격 - 그 앞은 선발. 경고 구간이 없으면 선발만 표시
    private string BuildCompositionText(WaveDefinition wave)
    {
        WaveDefinition.EnemySpawnSegment[] segments = wave.SpawnSegments;

        int rushStart = segments.Length;
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i].showWarning || segments[i].waitForClear) { rushStart = i; break; }
        }

        List<string> lines = new List<string>();
        if (rushStart > 0)
        {
            string line = "선발: " + DescribeCounts(CountByType(segments, 0, rushStart));
            if (CountTicks(segments, 0, rushStart) > 1) line += $" ({FormatSeconds(wave.SpawnInterval)}초 간격)";
            lines.Add(line);
        }
        if (rushStart < segments.Length)
        {
            lines.Add("돌격: " + DescribeRush(wave, rushStart));
        }
        return string.Join("\n", lines);
    }

    // 돌격 구간들 - 대기 0.5초 이하는 앞 구간과 동시, 그 외는 "N초 뒤"(대기가 0이면 웨이브 기본 간격 뒤)
    private string DescribeRush(WaveDefinition wave, int rushStart)
    {
        WaveDefinition.EnemySpawnSegment[] segments = wave.SpawnSegments;
        List<int[]> groups = new List<int[]>();   // 각 묶음의 {헤비, 일반} 마릿수
        List<float> waits = new List<float>();    // 각 묶음이 앞 묶음 뒤 몇 초에 나오는지 (첫 묶음은 0)

        for (int i = rushStart; i < segments.Length; i++)
        {
            WaveDefinition.EnemySpawnSegment segment = segments[i];
            bool sameMoment = i > rushStart && segment.delayBefore > 0f && segment.delayBefore <= 0.5f;

            if (groups.Count == 0 || !sameMoment)
            {
                groups.Add(new int[2]);
                waits.Add(groups.Count == 1 ? 0f : (segment.delayBefore > 0.5f ? segment.delayBefore : wave.SpawnInterval));
            }

            int enemies = segment.count * segment.directions.Length;
            groups[groups.Count - 1][segment.type == EnemyType.Heavy ? 0 : 1] += enemies;
        }

        List<string> parts = new List<string>();
        for (int g = 0; g < groups.Count; g++)
        {
            string text = DescribeCounts(groups[g]);
            parts.Add(g == 0 ? text : $"{FormatSeconds(waits[g])}초 뒤 {text}");
        }
        return string.Join(" → ", parts);
    }

    private int[] CountByType(WaveDefinition.EnemySpawnSegment[] segments, int from, int to)
    {
        int[] counts = new int[2];   // {헤비, 일반}
        for (int i = from; i < to; i++)
        {
            counts[segments[i].type == EnemyType.Heavy ? 0 : 1] += segments[i].count * segments[i].directions.Length;
        }
        return counts;
    }

    private int CountTicks(WaveDefinition.EnemySpawnSegment[] segments, int from, int to)
    {
        int ticks = 0;
        for (int i = from; i < to; i++)
        {
            ticks += Mathf.CeilToInt((float)segments[i].count / segments[i].simultaneousCount);
        }
        return ticks;
    }

    private string DescribeCounts(int[] counts)
    {
        List<string> parts = new List<string>();
        if (counts[0] > 0) parts.Add($"헤비 {counts[0]}");
        if (counts[1] > 0) parts.Add($"일반 {counts[1]}");
        return string.Join(" + ", parts);
    }

    private string FormatSeconds(float seconds)
    {
        return seconds.ToString("0.#", CultureInfo.InvariantCulture);
    }

    // 모든 구간 방향의 합집합, 헤비 구간 방향이 다르면 따로 덧붙임
    private string BuildDirectionText(WaveDefinition wave)
    {
        bool[] all = new bool[DirectionNames.Length];
        bool[] heavy = new bool[DirectionNames.Length];
        bool hasHeavy = false;

        foreach (WaveDefinition.EnemySpawnSegment segment in wave.SpawnSegments)
        {
            foreach (BarricadeDirection direction in segment.directions)
            {
                all[(int)direction] = true;
                if (segment.type == EnemyType.Heavy)
                {
                    heavy[(int)direction] = true;
                    hasHeavy = true;
                }
            }
        }

        string text = $"{CountTrue(all)}방향 ({JoinDirections(all)})";
        if (hasHeavy && JoinDirections(heavy) != JoinDirections(all))
        {
            text += $" / 헤비: {JoinDirections(heavy)}";
        }
        return text;
    }

    private int CountTrue(bool[] flags)
    {
        int count = 0;
        foreach (bool flag in flags) if (flag) count++;
        return count;
    }

    private string JoinDirections(bool[] flags)
    {
        List<string> names = new List<string>();
        for (int i = 0; i < flags.Length; i++)
        {
            if (flags[i]) names.Add(DirectionNames[i]);
        }
        return string.Join("·", names);
    }
}
