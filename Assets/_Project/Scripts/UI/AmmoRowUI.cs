using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoRowUI : MonoBehaviour
{
    [SerializeField] private int weaponIndex;           // PlayerWeapon 슬롯 번호 - 행마다 다르게 설정
    [SerializeField] private TMP_Text weaponNameText;   // "권총", "저격총"
    [SerializeField] private TMP_Text reserveText;      // "88 / 100"
    [SerializeField] private TMP_Text bundleCountText;  // "2"
    [SerializeField] private TMP_Text fillAmountText;   // "+12발"
    [SerializeField] private TMP_Text totalCostText;    // "100 크리스탈"
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button buyButton;

    private int bundleCount = 1;

    public int WeaponIndex => weaponIndex;

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
        PlayerWeapon.WeaponSlot slot = PlayerWeapon.Instance.GetSlot(weaponIndex);
        int maxBundles = slot.GetMaxAffordableBundles();
        bundleCount = Mathf.Clamp(bundleCount, 1, Mathf.Max(1,maxBundles));

        weaponNameText.text = $"탄약({slot.data.displayName})";
        reserveText.text = $"{slot.ReserveAmmo} / {slot.Stats.maxReserveAmmo}";
        bundleCountText.text = bundleCount.ToString();

        int fillAmount = slot.GetAmmoFillAmount(bundleCount);
        fillAmountText.text = $"+{fillAmount}발";

        int cost = slot.GetAmmoCost(bundleCount);
        totalCostText.text = $"{CurrencyManager.Format(cost)} 크리스탈";

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
        PlayerWeapon.WeaponSlot slot = PlayerWeapon.Instance.GetSlot(weaponIndex);
        int cost = slot.GetAmmoCost(bundleCount);
        ConfirmationDialog.Instance.Show($"{slot.data.displayName} 탄약을 {CurrencyManager.Format(cost)} 크리스탈로 구매하시겠습니까?", () =>
        {
            if (!PlayerWeapon.Instance.TryBuyAmmo(weaponIndex, bundleCount))
            {
                ToastMessage.Instance.Show("크리스탈이 부족합니다.");
                return;
            }
            bundleCount = 1;
            Refresh();
        });
    }
}
