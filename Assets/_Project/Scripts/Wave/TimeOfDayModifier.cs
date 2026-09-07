using UnityEngine;

// UI 노출용 등급이며 원시 수치는 의도적으로 숨김
public enum StrengthTier { Weak, Normal, Strong }

[CreateAssetMenu(fileName = "TimeOfDayModifier", menuName = "ZombieWaveDefence/Time Of Day Modifier")]
public class TimeOfDayModifier : ScriptableObject
{
    [Header("식별")]
    [SerializeField] private string timeOfDayName;

    [Header("배율")]
    [SerializeField] private float spawnCountMultiplier = 1f;
    [SerializeField] private float enemyStrengthMultiplier = 1f;
    [SerializeField] private float rareCrystalDropRateMultiplier = 1f;

    public string TimeOfDayName => timeOfDayName;
    public float SpawnCountMultiplier => spawnCountMultiplier;
    public float EnemyStrengthMultiplier => enemyStrengthMultiplier;
    public float RareCrystalDropRateMultiplier => rareCrystalDropRateMultiplier;

    public StrengthTier GetStrengthTier()
    {
        // 임계값은 임시로 정한값(확정 아님)
        if (enemyStrengthMultiplier < 0.9f) return StrengthTier.Weak;
        if (enemyStrengthMultiplier > 1.1f) return StrengthTier.Strong;
        return StrengthTier.Normal;
    }
}
