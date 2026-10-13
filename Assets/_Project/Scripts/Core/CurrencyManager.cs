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

    public bool TrySpend(int amount)
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
