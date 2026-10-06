using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PotionRowUI : MonoBehaviour
{
    private const int PotionIndex = 0; // 지금은 회복약 1종만 사용

    [SerializeField] private TMP_Text countText;      // "3 / 10"
    [SerializeField] private TMP_Text buyCountText;   // "2"
    [SerializeField] private TMP_Text totalCostText;  // "60 크리스탈"
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button buyButton;

    private int buyCount = 1;

    private void Awake()
    {
        decreaseButton.onClick.AddListener(OnDecreaseClicked);
        increaseButton.onClick.AddListener(OnIncreaseClicked);
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    private void OnEnable()
    {
        buyCount = 1;
    }

    public void Refresh()
    {
        int maxAffordable = PlayerConsumables.Instance.GetMaxAffordablePotions(PotionIndex);
        buyCount = Mathf.Clamp(buyCount, 1, Mathf.Max(1, maxAffordable));

        int count = PlayerConsumables.Instance.GetPotionCount(PotionIndex);
        int maxCount = PlayerConsumables.Instance.GetMaxPotionCount(PotionIndex);
        countText.text = $"{count} / {maxCount}";
        buyCountText.text = buyCount.ToString();

        int cost = PlayerConsumables.Instance.GetPotionBundleCost(PotionIndex, buyCount);
        totalCostText.text = $"{cost} 크리스탈";

        bool canBuy = maxAffordable > 0;
        decreaseButton.interactable = canBuy && buyCount > 1;
        increaseButton.interactable = canBuy && buyCount < maxAffordable;
        buyButton.interactable = canBuy;
    }

    private void OnDecreaseClicked()
    {
        buyCount--;
        Refresh();
    }

    private void OnIncreaseClicked()
    {
        buyCount++;
        Refresh();
    }

    private void OnBuyClicked()
    {
        int cost = PlayerConsumables.Instance.GetPotionBundleCost(PotionIndex, buyCount);
        ConfirmationDialog.Instance.Show($"{cost} 크리스탈로 회복약을 구매하시겠습니까?", () =>
        {
            if (!PlayerConsumables.Instance.TryBuyPotion(PotionIndex, buyCount))
            {
                ToastMessage.Instance.Show("크리스탈이 부족합니다.");
                return;
            }
            buyCount = 1;
            Refresh();
        });
    }
}
