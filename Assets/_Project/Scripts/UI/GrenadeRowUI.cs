using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GrenadeRowUI : MonoBehaviour
{
    private const int GrenadeIndex = 0;

    [SerializeField] private TMP_Text countText;        // "3 / 5"
    [SerializeField] private TMP_Text buyCountText;     // "2"
    [SerializeField] private TMP_Text totalCostText;    // "160 크리스탈"
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

    public void Refresh()
    {
        int maxAffordable = PlayerConsumables.Instance.GetMaxAffordableGrenades(GrenadeIndex);
        buyCount = Mathf.Clamp(buyCount, 1, Mathf.Max(1, maxAffordable));

        int count = PlayerConsumables.Instance.GetGrenadeCount(GrenadeIndex);
        int maxCount = PlayerConsumables.Instance.GetMaxGrenadeCount(GrenadeIndex);
        countText.text = $"{count} / {maxCount}";
        buyCountText.text = buyCount.ToString();

        int cost = PlayerConsumables.Instance.GetGrenadeBundleCost(GrenadeIndex, buyCount);
        totalCostText.text = $"{CurrencyManager.Format(cost)} 크리스탈";

        bool canBuy = maxAffordable > 0;
        decreaseButton.interactable = canBuy && buyCount > 1;
        increaseButton.interactable = canBuy && buyCount < maxAffordable;
        buyButton.interactable = canBuy;
    }

    private void OnEnable()
    {
        buyCount = 1;
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
        int cost = PlayerConsumables.Instance.GetGrenadeBundleCost(GrenadeIndex, buyCount);
        ConfirmationDialog.Instance.Show($"{CurrencyManager.Format(cost)} 크리스탈로 수류탄을 구매하시겠습니까?", () =>
        {
            if (!PlayerConsumables.Instance.TryBuyGrenade(GrenadeIndex, buyCount))
            {
                ToastMessage.Instance.Show("크리스탈이 부족합니다.");
                return;
            }
            buyCount = 1;
            Refresh();
        });
    }
}
