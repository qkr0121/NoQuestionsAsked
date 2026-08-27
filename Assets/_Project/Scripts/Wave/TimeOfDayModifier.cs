using UnityEngine;

public enum StrengthTier { Weak, Normal, Strong }

[CreateAssetMenu(fileName = "TimeOfDayModifier", menuName = "ZombieWaveDefence/Time Of DayModifier")]
public class TimeOfDayModifier : ScriptableObject
{
    [SerializeField] private string timeOfDayName;
    [SerializeField] private float spawnCountMultiplier = 1f;
    [SerializeField] private float enemyStrengthMultiplier = 1f;
    [SerializeField] private float rareCrystalDropRateMultiplier = 1f;

    public string TimeOfDayName => timeOfDayName;
    public float SpawnCountMultiplier => spawnCountMultiplier;
    public float EnemyStrengthMultiplier => enemyStrengthMultiplier;
    public float RareCrystalDropRateMultiplier => rareCrystalDropRateMultiplier;

    public StrengthTier GetStrengthTier()
    {
        if (enemyStrengthMultiplier < 0.9f) return StrengthTier.Weak;
        if (enemyStrengthMultiplier > 1.1f) return StrengthTier.Strong;
        return StrengthTier.Normal;
    }
}
