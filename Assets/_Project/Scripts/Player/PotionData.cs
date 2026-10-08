using UnityEngine;

[CreateAssetMenu(menuName = "Data/Potion Data")]
public class PotionData : ScriptableObject
{
    public int healAmount = 30;
    public int bundleCost = 30;
    public int maxCount = 10;
    public float cooldown = 3f;   // 즉시 회복이라 쿨타임이 연속 사용을 막는 유일한 제약
}
