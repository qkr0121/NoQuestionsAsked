using UnityEngine;

public enum SpawnDirectionPattern { Four, Eight } // 8방향은 로직만 있고 실제 스폰포인트는 미생성

[CreateAssetMenu(fileName = "WaveDefinition", menuName = "ZombieWaveDefence/WaveDefinition")]
public class WaveDefinition : ScriptableObject
{
    [Header("식별")]
    [SerializeField] private string waveName;

    [System.Serializable]
    public class EnemySpawnSegment
    {
        public EnemyType type;
        public int count;
    }

    [Header("스폰 구성")]
    [SerializeField] private EnemySpawnSegment[] spawnSegments; // 순서대로 스폰됨 (예: Heavy 10 -> Normal 10 -> Heavy 10)
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private SpawnDirectionPattern spawnDirections = SpawnDirectionPattern.Four;

    [Header("보상")]
    [SerializeField] private float rareCrystalDropChance = 0.05f;

    public string WaveName => waveName;
    public EnemySpawnSegment[] SpawnSegments => spawnSegments;
    public float SpawnInterval => spawnInterval;
    public SpawnDirectionPattern SpawnDirections => spawnDirections;
    public float RareCrystalDropChance => rareCrystalDropChance;
}
