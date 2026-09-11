using UnityEngine;

public class BarricadeSlot : MonoBehaviour
{
    [SerializeField] private BarricadeDirection direction;
    [SerializeField] private GameObject barricadeModel;

    public BarricadeDirection Direction => direction;

    private void OnEnable()
    {
        BarricadeManager.Instance.OnSlotChanged += HandleSlotChanged;
        Refresh();
    }

    private void OnDisable()
    {
        BarricadeManager.Instance.OnSlotChanged -= HandleSlotChanged;
    }

    public void TakeDamage(int amount)
    {
        BarricadeManager.Instance.TakeDamage(direction, amount);
    }

    private void HandleSlotChanged(BarricadeDirection changedDirection)
    {
        if (changedDirection != direction) return;
        Refresh();
    }

    private void Refresh()
    {
        BarricadeSlotInfo info = BarricadeManager.Instance.GetSlotInfo(direction);
        barricadeModel.SetActive(info.State == BarricadeState.Alive);
    }
}
