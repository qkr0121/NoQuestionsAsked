using System;
using System.Collections.Generic;
using UnityEngine;

public class BarricadeManager : MonoBehaviour
{
    public static BarricadeManager Instance { get; private set; }

    [SerializeField] private BarricadeData barricadeData;

    private class SlotData
    {
        public BarricadeState State = BarricadeState.Empty;
        public int CurrentLevel;
        public int CurrentDurability;
    }

    private readonly Dictionary<BarricadeDirection, SlotData> slots = new();

    public event Action<BarricadeDirection> OnSlotChanged;

    private void Awake()
    {
        Instance = this;

        foreach(BarricadeDirection direction in System.Enum.GetValues(typeof(BarricadeDirection)))
        {
            slots[direction] = new SlotData();
        }
    }

    public BarricadeSlotInfo GetSlotInfo(BarricadeDirection direction)
    {
        SlotData data = slots[direction];
        int maxDurability = data.CurrentLevel > 0 ? barricadeData.maxDurabilityByLevel[data.CurrentLevel - 1] : 0;

        return new BarricadeSlotInfo(data.State, data.CurrentLevel, data.CurrentDurability, maxDurability);
    }

    public int MaxLevel => barricadeData.MaxLevel;

    public int InstallCost(int level) => barricadeData.installCostByLevel[level - 1];

    public int GetRepairCost(BarricadeDirection direction)
    {
        SlotData data = slots[direction];
        int maxDurability = barricadeData.maxDurabilityByLevel[data.CurrentLevel - 1];
        return Mathf.RoundToInt((1f - (float)data.CurrentDurability / maxDurability) * InstallCost(data.CurrentLevel));
    }

    public bool IsLevelUnlocked(int level)
    {
        if (level == 1) return true;
        return BlueprintManager.Instance.IsUnlocked(LevelToBlueprintId(level));
    }

    public bool TryInstall(BarricadeDirection direction, int level)
    {
        if (!IsLevelUnlocked(level)) return false;
        if (!CurrencyManager.Instance.TrySpend(InstallCost(level))) return false;

        SlotData data = slots[direction];
        data.State = BarricadeState.Alive;
        data.CurrentLevel = level;
        data.CurrentDurability = barricadeData.maxDurabilityByLevel[level - 1];

        OnSlotChanged?.Invoke(direction);
        return true;
    }

    public bool TryRepair(BarricadeDirection direction)
    {
        SlotData data = slots[direction];
        if (data.State == BarricadeState.Empty) return false;

        int maxDurability = barricadeData.maxDurabilityByLevel[data.CurrentLevel - 1];
        if (data.CurrentDurability >= maxDurability) return false;
        if (!CurrencyManager.Instance.TrySpend(GetRepairCost(direction))) return false;

        data.State = BarricadeState.Alive;
        data.CurrentDurability = maxDurability;

        OnSlotChanged?.Invoke(direction);
        return true;
    }

    public void TakeDamage(BarricadeDirection direction, int amount)
    {
        SlotData data = slots[direction];
        if (data.State != BarricadeState.Alive) return;

        data.CurrentDurability -= amount;
        Debug.Log($"[방어벽] {direction} 피격: {amount} 데미지, 남은 내구도 {data.CurrentDurability}/{GetSlotInfo(direction).MaxDurability}");

        if(data.CurrentDurability <=0)
        {
            data.State = BarricadeState.Destroyed;
            Debug.Log($"[방어벽] {direction} 파괴됨");
        }

        OnSlotChanged?.Invoke(direction);
    }

    public void ResetAllSlots()
    {
        foreach (BarricadeDirection direction in new List<BarricadeDirection>(slots.Keys))
        {
            slots[direction] = new SlotData();
            OnSlotChanged?.Invoke(direction);
        }
    }

    public void LoadFrom(BarricadeSaveEntry[] entries)
    {
        // 저장에 없는 값이 남지 않도록 먼저 전부 비움
        ResetAllSlots();
        if (entries == null) return;   // 이 필드가 생기기 전에 만든 세이브 파일

        BarricadeDirection[] directions = (BarricadeDirection[])Enum.GetValues(typeof(BarricadeDirection));
        int count = Mathf.Min(entries.Length, directions.Length);
        for (int i = 0; i < count; i++)
        {
            BarricadeSaveEntry entry = entries[i];

            // 밸런스를 수정한 뒤에도 옛 세이브가 범위를 넘는 값을 만들지 못하게 검증함
            if (!Enum.IsDefined(typeof(BarricadeState), entry.state)) continue;
            BarricadeState state = (BarricadeState)entry.state;
            if (state == BarricadeState.Empty) continue;
            if (entry.level < 1 || entry.level > MaxLevel) continue;

            SlotData data = slots[directions[i]];
            data.State = state;
            data.CurrentLevel = entry.level;
            data.CurrentDurability = Mathf.Clamp(entry.durability, 0, barricadeData.maxDurabilityByLevel[entry.level - 1]);
            OnSlotChanged?.Invoke(directions[i]);
        }
    }

    private BlueprintId LevelToBlueprintId(int level)
    {
        switch(level)
        {
            case 2: return BlueprintId.BarricadeLevel2;
            case 3: return BlueprintId.BarricadeLevel3;
            default: throw new ArgumentOutOfRangeException(nameof(level));
        }
    }
}
