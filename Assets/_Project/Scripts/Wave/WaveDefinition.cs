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
        public BarricadeDirection[] directions;
        public int simultaneousCount = 1; // 방향당 한 번에 동시에 스폰할 마릿수
        public float delayBefore;         // 이전 스폰 후 이 구간 첫 스폰까지의 대기(초), 0이면 기본 간격 - 이 동안 스폰이 멈춤 (돌격 전 준비 시간)
        public bool showWarning;          // true면 대기가 시작될 때 경고 표시 (플레이어가 방어벽으로 물러날 신호)
        public bool waitForClear;         // true면 이 구간 시작 전에 필드의 모든 좀비를 처치할 때까지 기다림 (처치한 뒤에 delayBefore 대기와 경고가 시작됨)
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
