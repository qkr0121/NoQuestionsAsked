using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;           // 무기 칸만 연결 - 수류탄·회복약 칸은 비워 두면 기존 그대로
    [SerializeField] private TMP_Text reserveText;      // 예비 탄약 (오른쪽 위)
    [SerializeField] private TMP_Text mainText;         // 탄창 탄약 또는 보유 개수 (아래)
    [SerializeField] private GameObject selectedMark;
    [SerializeField] private GameObject lockedMark;     // 잠긴 칸을 덮는 반투명 오브젝트
    [SerializeField] private GameObject cooldownMark;   // 회복약 칸만 연결 - 쿨타임 중 어둡게
    [SerializeField] private TMP_Text cooldownText;     // 회복약 칸만 연결
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lowAmmoColor = Color.red;

    // 탄창이 이 비율 이하로 남으면 재장전을 유도하려고 색을 바꿈
    private const float LowAmmoRatio = 0.2f;

    // 수류탄·회복약 칸용 - 무기 칸은 ShowWeapon 이 직접 설정함
    public void SetIcon(Sprite icon)
    {
        if (iconImage == null) return;
        iconImage.sprite = icon;
        iconImage.enabled = icon != null;   // 아이콘이 없으면 빈 이미지(흰 칸)가 보이지 않게
    }

    public void ShowLocked()
    {
        if (iconImage != null) iconImage.enabled = false;   // 잠긴 칸에 이전 무기 아이콘이 남지 않도록
        lockedMark.SetActive(true);
        selectedMark.SetActive(false);
        reserveText.text = "";
        mainText.text = "";
    }

    public void ShowWeapon(PlayerWeapon.WeaponSlot slot, bool selected)
    {
        if (iconImage != null)
        {
            iconImage.sprite = slot.data.icon;
            iconImage.enabled = slot.data.icon != null;   // 아이콘이 없는 무기는 빈 이미지(흰 칸)가 보이지 않게
        }
        lockedMark.SetActive(false);
        selectedMark.SetActive(selected);
        reserveText.text = slot.ReserveAmmo.ToString();
        mainText.text = slot.MagazineAmmo.ToString();
        mainText.color = slot.MagazineAmmo <= slot.Stats.magazineSize * LowAmmoRatio ? lowAmmoColor : normalColor;
    }

    public void ShowCount(int count, bool selected)
    {
        lockedMark.SetActive(false);
        selectedMark.SetActive(selected);
        reserveText.text = "";
        mainText.text = count.ToString();
    }

    public void ShowPotion(int count, float cooldownRemaining)
    {
        ShowCount(count, false);

        bool cooling = cooldownRemaining > 0f;
        cooldownMark.SetActive(cooling);
        if (cooling) cooldownText.text = cooldownRemaining.ToString("0.0");
    }
}
