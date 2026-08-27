using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text crystalText;
    [SerializeField] private TMP_Text healthText;

    private void Start()
    {
        UIManager.Instance.RegisterGameUI(this);
    }

    private void Update()
    {
        crystalText.text = $"Crystal: {CurrencyManager.Instance.Crystal}";
        healthText.text = $"HP: {PlayerManager.Instance.CurrentHealth} / {PlayerManager.Instance.MaxHealth}";
    }
    
}
