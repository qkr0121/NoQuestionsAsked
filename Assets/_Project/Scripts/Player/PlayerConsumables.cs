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
        return true;
    }

    public PotionData GetPotionData(int potionIndex) => potionTypes[potionIndex];

    public bool TryConsumePotion(int potionIndex)
    {
        if (potionCounts[potionIndex] <= 0) return false;
        potionCounts[potionIndex]--;
        return true;
    }

    public int[] GetGrenadeCounts() => (int[])grenadeCounts.Clone();
    public int[] GetPotionCounts() => (int[])potionCounts.Clone();

    public void LoadCounts(int[] savedGrenades, int[] savedPotions)
    {
        RestoreCounts(savedGrenades, grenadeCounts);
        RestoreCounts(savedPotions, potionCounts);
    }

    public void ResetCounts()
    {
        System.Array.Clear(grenadeCounts, 0, grenadeCounts.Length);
        System.Array.Clear(potionCounts, 0, potionCounts.Length);
    }

    private static void RestoreCounts(int[] saved, int[] target)
    {
        System.Array.Clear(target, 0, target.Length);
        if (saved == null) return;   // 이 필드가 생기기 전에 만든 세이브 파일

        // 종류 수가 바뀐 세이브도 안전하도록 겹치는 부분만 복원
        int count = Mathf.Min(saved.Length, target.Length);
        System.Array.Copy(saved, target, count);
    }
}
