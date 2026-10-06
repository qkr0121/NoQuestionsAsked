using UnityEngine;

[CreateAssetMenu(menuName = "Data/Grenade Data")]
public class GrenadeData : ScriptableObject
{
    [Header("연출")]
    public Grenade prefab;

    [Header("폭발")]
    public int damage = 30;
    public float explosionRadius = 3f;
    public float fuseTime = 1f;         // 착탄 후 폭발까지 시간

    [Header("투척")]
    public float maxThrowRange = 12f;
    public float flightTime = 0.8f;     // 거리와 무관하게 고정 - 멀리 던질수록 빠르게 날아감
    public float arcHeight = 3f;        // 포물선 최고점 높이

    [Header("구매")]
    public int bundleCost = 80;
    public int maxCount = 5;
}
