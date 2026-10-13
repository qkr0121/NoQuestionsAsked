using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private int crystalPerKill = 10;
    [SerializeField] private int startingCrystal = 0; // 테스트용 - 빌드 전 0으로 되돌릴 것

    public int Crystal { get; private set; }

    private void Awake()
    {
        Instance = this;
        Crystal = startingCrystal;
    }

    public void AddKillReward()
    {
        Crystal += crystalPerKill;
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
