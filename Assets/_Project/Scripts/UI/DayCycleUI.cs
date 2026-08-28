using UnityEngine;
using TMPro;

public class DayCycleUI : MonoBehaviour
{
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text slotText;

    private void Update()
    {
        dayText.text = $"Day {RunState.Instance.CurrentDay}";
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
