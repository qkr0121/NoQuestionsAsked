using UnityEngine;

public class PlayerConsumables : MonoBehaviour
{
    public static PlayerConsumables Instance { get; private set; }

    [SerializeField] private GrenadeData[] grenadeTypes; // 지금은 0번째(기본 수류탄)만 사용
    [SerializeField] private PotionData[] potionTypes;   // 지금은 0번째(기본 회복약)만 사용

    private int[] grenadeCounts;
    private int[] potionCounts;

    private void Awake()
    {
        Instance = this;
        grenadeCounts = new int[grenadeTypes.Length];
        potionCounts = new int[potionTypes.Length];
    }

    public int GrenadeTypeCount => grenadeTypes.Length;
    public GrenadeData GetGrenadeData(int grenadeIndex) => grenadeTypes[grenadeIndex];
    public int GetGrenadeCount(int grenadeIndex) => grenadeCounts[grenadeIndex];
    public int GetMaxGrenadeCount(int grenadeIndex) => grenadeTypes[grenadeIndex].maxCount;
    public int GetGrenadeBundleCost(int grenadeIndex, int count) => grenadeTypes[grenadeIndex].bundleCost * count;

    public int GetMaxAffordableGrenades(int grenadeIndex)
    {
        int needed = grenadeTypes[grenadeIndex].maxCount - grenadeCounts[grenadeIndex];
        return Mathf.Max(0, needed);
    }

    public bool TryBuyGrenade(int grenadeIndex, int count)
    {
        GrenadeData data = grenadeTypes[grenadeIndex];
        int cost = data.bundleCost * count;
        if (!CurrencyManager.Instance.TrySpend(cost)) return false;

        grenadeCounts[grenadeIndex] += count;
        Debug.Log($"{data.name} {count}개 구매 완료. 현재 보유: {grenadeCounts[grenadeIndex]}/{data.maxCount}");
        return true;
    }

    public bool TryConsumeGrenade(int grenadeIndex)
    {
        if (grenadeCounts[grenadeIndex] <= 0) return false;
        grenadeCounts[grenadeIndex]--;
        return true;
    }

    public int GetPotionCount(int potionIndex) => potionCounts[potionIndex];
    public int GetMaxPotionCount(int potionIndex) => potionTypes[potionIndex].maxCount;
    public int GetPotionBundleCost(int potionIndex, int count) => potionTypes[potionIndex].bundleCost * count;

    public int GetMaxAffordablePotions(int potionIndex)
    {
        int needed = potionTypes[potionIndex].maxCount - potionCounts[potionIndex];
        return Mathf.Max(0, needed);
    }

    public bool TryBuyPotion(int potionIndex, int count)
    {
        PotionData data = potionTypes[potionIndex];
        int cost = data.bundleCost * count;
        if (!CurrencyManager.Instance.TrySpend(cost)) return false;

        potionCounts[potionIndex] += count;
        Debug.Log($"{data.name} {count}개 구매 완료. 현재 보유: {potionCounts[potionIndex]}/{data.maxCount}");
        return true;
    }
}
