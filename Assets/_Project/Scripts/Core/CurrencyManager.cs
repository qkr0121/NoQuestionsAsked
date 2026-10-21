using System.Globalization;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    // 모든 크리스탈 표시를 한곳에서 포맷 - 문화권 설정과 무관하게 항상 12,000 형태로 보이게 함
    public static string Format(int amount)
    {
        return amount.ToString("N0", CultureInfo.InvariantCulture);
    }

    [SerializeField] private int startingCrystal = 0; // 테스트용 - 빌드 전 0으로 되돌릴 것

    [Header("소리")]
    [SerializeField] private SoundData purchaseSound;       // 구매 성공
    [SerializeField] private SoundData purchaseFailSound;   // 크리스탈 부족
    [SerializeField] private SoundData upgradeSound;        // 강화 성공
    public SoundData UpgradeSound => upgradeSound;

    public int SoundCount { get; private set; }   // 구매·강화·실패 소리를 낸 횟수 - 확인창이 클릭음을 낼지 판단하는 데 씀

    public int Crystal { get; private set; }

    private void Awake()
    {
        Instance = this;
        Crystal = startingCrystal;
    }

    public void AddKillReward(int amount)
    {
        Crystal += amount;
    }

    public bool TrySpend(int amount, SoundData successSound = null)
    {
        if (Crystal < amount)
        {
            SoundCount++;
            AudioManager.PlaySfx(purchaseFailSound);
            return false;
        }

        Crystal -= amount;
        SoundCount++;
        AudioManager.PlaySfx(successSound != null ? successSound : purchaseSound);
        return true;
    }

    // 미션 상납처럼 구매가 아닌 지출은 구매 소리를 내지 않음
    public bool TrySpendQuietly(int amount)
    {
        if (Crystal < amount) return false;

        Crystal -= amount;
        return true;
    }

    public void LoadCrystal(int amount)
    {
        Crystal = amount;
    }

    public void ResetCrystal()
    {
        Crystal = startingCrystal;   // 새 런도 시작 크리스탈에서 출발 - 테스트 값을 쓰려면 이 필드를 올리면 됨
    }
}
