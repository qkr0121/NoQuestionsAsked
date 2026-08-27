using UnityEngine;

public enum SpawnDirectionPattern { Four, Eight }

[CreateAssetMenu(fileName = "WaveDefinition", menuName = "ZombieWaveDefence/WaveDefinition")]
public class WaveDefinition : ScriptableObject
{
    [SerializeField] private string waveName;
    [SerializeField] private int totalEnemyCount = 20;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private SpawnDirectionPattern spawnDirections = SpawnDirectionPattern.Four;
    [SerializeField] private float rareCrystalDropChance = 0.05f;

    public string WaveName => waveName;
    public int TotalEnemyCount => totalEnemyCount;
    public float SpawnInterval => spawnInterval;
    public SpawnDirectionPattern SpawnDirections => spawnDirections;
    public float RareCrystalDropChance => rareCrystalDropChance;
}
