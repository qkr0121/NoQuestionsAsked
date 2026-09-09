using UnityEngine;

public class BarricadeManager : MonoBehaviour
{
    public static BarricadeManager Instance { get; private set; }

    [SerializeField] private BarricadeSlot[] barricadeSlots;

    private void Awake()
    {
        Instance = this;
    }

    public BarricadeSlot GetSlot(BarricadeDirection direction)
    {
        foreach(BarricadeSlot slot in barricadeSlots)
        {
            if (slot.Direction == direction) return slot;
        }
        return null;
    }

    public BarricadeSlot[] AllSlots => barricadeSlots;

    public void ResetAllSlots()
    {
        foreach(BarricadeSlot slot in barricadeSlots)
        {
            slot.ResetSlot();
        }
    }
}
