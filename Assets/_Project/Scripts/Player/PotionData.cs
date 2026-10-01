using UnityEngine;

[CreateAssetMenu(menuName = "Data/Potion Data")]
public class PotionData : ScriptableObject
{
    public int healAmount = 30;
    public int bundleCost = 30;
    public int maxCount = 10;
}
