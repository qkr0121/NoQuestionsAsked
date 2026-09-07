using UnityEngine;

public enum SpawnDirectionPattern { Four, Eight } // 8방향은 로직만 있고 실제 스폰포인트는 미생성

[CreateAssetMenu(fileName = "WaveDefinition", menuName = "ZombieWaveDefence/WaveDefinition")]
public class WaveDefinition : ScriptableObject
{
    [Header("식별")]
    [SerializeField] private string waveName;

    [Header("스폰 구성")]
    [SerializeField] private int totalEnemyCount = 20;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private SpawnDirectionPattern spawnDirections = SpawnDirectionPattern.Four;

    [Header("보상")]
    [SerializeField] private float rareCrystalDropChance = 0.05f;

    public string WaveName => waveName;
    public int TotalEnemyCount => totalEnemyCount;
    public float SpawnInterval => spawnInterval;
    public SpawnDirectionPattern SpawnDirections => spawnDirections;
    public float RareCrystalDropChance => rareCrystalDropChance;
}
