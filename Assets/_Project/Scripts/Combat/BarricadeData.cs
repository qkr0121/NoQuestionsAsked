using UnityEngine;

[CreateAssetMenu(menuName = "Data/Wall Data")]
public class BarricadeData : ScriptableObject
{
    [Header("레벨별 스탯 (인덱스 0 = 1레벨")]
    public int[] maxDurabilityByLevel = new int[3];
    public int[] installCostByLevel = new int[3];

    [Header("소리")]
    public SoundData destroySound;   // 내구도가 0이 되어 부서질 때, 비워도 동작

    public int MaxLevel => maxDurabilityByLevel.Length;
}
