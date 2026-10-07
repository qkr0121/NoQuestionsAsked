using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text reserveText;      // "88 / 100"
    [SerializeField] private TMP_Text bundleCountText;  // "2"
    [SerializeField] private TMP_Text fillAmountText;   // "+12발"
    [SerializeField] private TMP_Text totalCostText;    // "100 크리스탈"
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button buyButton;

    private int bundleCount = 1;

    private void Awake()
    {
        decreaseButton.onClick.AddListener(OnDecreaseClicked);
        increaseButton.onClick.AddListener(OnIncreaseClicked);
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    private void OnEnable()
    {
        bundleCount = 1;
    }

    public void Refresh()
    {
        int maxBundles = PlayerWeapon.Instance.GetMaxAffordableBundles();
        bundleCount = Mathf.Clamp(bundleCount, 1, Mathf.Max(1,maxBundles));

        reserveText.text = $"{PlayerWeapon.Instance.ReserveAmmo} / {PlayerWeapon.Instance.MaxReserveAmmo}";
        bundleCountText.text = bundleCount.ToString();

        int fillAmount = PlayerWeapon.Instance.GetAmmoFillAmount(bundleCount);
        fillAmountText.text = $"+{fillAmount}발";

        int cost = PlayerWeapon.Instance.GetAmmoCost(bundleCount);
        totalCostText.text = $"{cost} 크리스탈";

        bool canBuy = maxBundles > 0;
        decreaseButton.interactable = canBuy && bundleCount > 1;
        increaseButton.interactable = canBuy && bundleCount < maxBundles;
        buyButton.interactable = canBuy;
    }

    private void OnDecreaseClicked()
    {
        bundleCount--;
        Refresh();
    }

    private void OnIncreaseClicked()
    {
        bundleCount++;
        Refresh();
    }

    private void OnBuyClicked()
    {
        int cost = PlayerWeapon.Instance.GetAmmoCost(bundleCount);
        ConfirmationDialog.Instance.Show($"{cost} 크리스탈로 탄약을 구매하시겠습니까?", () =>
        {
            if (!PlayerWeapon.Instance.TryBuyAmmo(bundleCount))
            {
                ToastMessage.Instance.Show("크리스탈이 부족합니다.");
                return;
            }
            bundleCount = 1;
            Refresh();
        });
    }
}
