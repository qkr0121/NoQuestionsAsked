using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BarricadeRowUI : MonoBehaviour
{
    [SerializeField] private BarricadeDirection direction;
    [SerializeField] private Image rowBackground;
    [SerializeField] private TMP_Text directionText;
    [SerializeField] private TMP_Text durabilityText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Dropdown levelDropdown;
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private Button actionButton;

    private BarricadeSlotInfo info;

    private void Awake()
    {
        //actionButton.onClick.AddListener();
    }

    public void Refresh()
    {
        info = BarricadeManager.Instance.GetSlotInfo(direction);

        directionText.text = direction.ToString();
    }

    private void BuildLevelDropdown()
    {

    }

    private void RefreshDurabilityDisplay()
    {

    }

    private int GetSelectedLevel()
    {
        return 1;
    }

    private void RefreshActionButton()
    {

    }

    private void OnActionClicked()
    {

    }
}
