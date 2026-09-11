public class BarricadeSlotInfo
{
    public BarricadeState State { get; }
    public int CurrentLevel { get; }
    public int CurrentDurability { get; }
    public int MaxDurability { get; }

    public BarricadeSlotInfo(BarricadeState state, int level, int durability, int maxDurability)
    {
        State = state;
        CurrentLevel = level;
        CurrentDurability = durability;
        MaxDurability = maxDurability;
    }
    
}
