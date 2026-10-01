using UnityEngine;

public class PlayerConsumables : MonoBehaviour
{
    public static PlayerConsumables Instance { get; private set; }

    [SerializeField] private PotionData[] potionTypes;   // 지금은 0번째(기본 회복약)만 사용

    private int[] potionCounts;

    private void Awake()
    {
        Instance = this;
        potionCounts = new int[potionTypes.Length];
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

        int amount = Mathf.Min(count, data.maxCount - potionCounts[potionIndex]);
        potionCounts[potionIndex] += amount;
        Debug.Log($"{data.name} {amount}개 구매 완료. 현재 보유: {potionCounts[potionIndex]}/{data.maxCount}");
        return true;
    }
}
