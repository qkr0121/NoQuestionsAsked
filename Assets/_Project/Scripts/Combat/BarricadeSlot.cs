using UnityEngine;

public enum BarricadeDirection { North, East, South, West }
public enum BarricadeState { Empty, Alive, Destroyed }

public class BarricadeSlot : MonoBehaviour
{
    [SerializeField] private BarricadeDirection direction;
    [SerializeField] private BarricadeData barricadeData;
    [SerializeField] private GameObject barricadeModel;

    public BarricadeDirection Direction => direction;
    public BarricadeState State { get; private set; } = BarricadeState.Empty;
    public int CurrentLevel { get; private set; }
    public int CurrentDurability { get; private set; }

    public int MaxDurability => barricadeData.maxDurabilityByLevel[CurrentLevel - 1];
    public int InstallCost(int level) => barricadeData.installCostByLevel[level - 1];
    public int RepairCost => Mathf.RoundToInt((1f - (float)CurrentDurability / MaxDurability) * InstallCost(CurrentLevel));

    public bool IsLevelUnlocked(int level)
    {
        if (level == 1) return true;
        return BlueprintManager.Instance.IsUnlocked(LevelToBlueprintId(level));
    }

    public bool TryInstall(int level)
    {
        if (State != BarricadeState.Empty) return false;
        if (!IsLevelUnlocked(level)) return false;
        if (!CurrencyManager.Instance.TrySpend(InstallCost(level))) return false;

        State = BarricadeState.Alive;
        CurrentLevel = level;
        CurrentDurability = barricadeData.maxDurabilityByLevel[level - 1];
        barricadeModel.SetActive(true);
        return true;
    }

    public bool TryRepair()
    {
        if (State == BarricadeState.Empty) return false;
        if (CurrentDurability >= MaxDurability) return false;
        if (!CurrencyManager.Instance.TrySpend(RepairCost)) return false;

        State = BarricadeState.Alive;
        CurrentDurability = MaxDurability;
        return true;
    }

    public void TakeDamage(int amount)
    {
        if (State != BarricadeState.Alive) return;

        CurrentDurability -= amount;
        if(CurrentDurability <= 0)
        {
            State = BarricadeState.Destroyed;
            barricadeModel.SetActive(false);
        }
    }

    public void LoadFrom(BarricadeState state, int level, int durability)
    {
        State = state;
        CurrentLevel = level;
        CurrentDurability = durability;
        barricadeModel.SetActive(state == BarricadeState.Alive);
    }

    public void ResetSlot()
    {
        State = BarricadeState.Empty;
        CurrentLevel = 0;
        CurrentDurability = 0;
        barricadeModel.SetActive(false);
    }

    private BlueprintId LevelToBlueprintId(int level)
    {
        switch(level)
        {
            case 2: return BlueprintId.BarricadeLevel2;
            case 3: return BlueprintId.BarricadeLevel3;
            case 4: return BlueprintId.BarricadeLevel4;
            case 5: return BlueprintId.BarricadeLevel5;
            default: throw new System.ArgumentOutOfRangeException(nameof(level));
        }    
    }
}
