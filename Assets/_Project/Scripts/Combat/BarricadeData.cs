using UnityEngine;

[CreateAssetMenu(menuName = "Data/Wall Data")]
public class BarricadeData : ScriptableObject
{
    [Header("레벨별 스탯 (인덱스 0 = 1레벨")]
    public int[] maxDurabilityByLevel = new int[3];
    public int[] installCostByLevel = new int[3];

    public int MaxLevel => maxDurabilityByLevel.Length;
}
