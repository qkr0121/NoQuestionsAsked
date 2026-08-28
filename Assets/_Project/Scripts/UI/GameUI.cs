using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text crystalText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text slotText;

    private void Start()
    {
        UIManager.Instance.RegisterGameUI(this);
    }

    private void Update()
    {
        crystalText.text = $"Crystal: {CurrencyManager.Instance.Crystal}";
        healthText.text = $"HP: {PlayerManager.Instance.CurrentHealth} / {PlayerManager.Instance.MaxHealth}";
        dayText.text = $"DAY {RunState.Instance.CurrentDay}";
        slotText.text = GetSlotLabel();
    }
    
    private string GetSlotLabel()
    {
        return RunState.Instance.CurrentSlot switch
        {
            TimeSlot.Morning => "아침",
            TimeSlot.Day => "낮",
            TimeSlot.Night => "밤",
            _ => ""
        };
    }
}
