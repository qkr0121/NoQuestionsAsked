using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    private class SpawnEvent
    {
        public EnemyType type;
        public BarricadeDirection direction;
    }

    [SerializeField] private Transform[] spawnPoints; // 순서: North, East, South, West(BarricadeDirection 과 일치)

    private Transform player;
    private WaveDefinition waveDefinition;
    private TimeOfDayModifier timeOfDayModifier;

    private Queue<List<SpawnEvent>> spawnQueue; // 한 틱마다 동시에 나갈 좀비 묶음
    private int aliveCount;
    private float spawnTimer;
    private bool spawningStarted;

    public bool IsCleared { get; private set; }
    public bool CanExit => !spawningStarted || IsCleared;
    // 아직 웨이브를 시작 안했거나(웨이브 미선택 상태), 이미 클리어했으면 나갈 수 있음

    public void BeginWave(WaveDefinition definition, TimeOfDayModifier modifier)
    {
        waveDefinition = definition;
        timeOfDayModifier = modifier;

        spawnQueue = new Queue<List<SpawnEvent>>();
        foreach (WaveDefinition.EnemySpawnSegment segment in definition.SpawnSegments)
        {
            int scaledCount = Mathf.Max(1, Mathf.RoundToInt(segment.count * modifier.SpawnCountMultiplier));
            int tickCount = Mathf.CeilToInt((float)scaledCount / segment.simultaneousCount); // 이 구간이 몇 번의 틱으로 나뉘는지

            for(int tick = 0; tick < tickCount; tick++)
            {
                int spawnedSoFar = tick * segment.simultaneousCount;
                int remainingInThisDirection = Mathf.Min(segment.simultaneousCount, scaledCount - spawnedSoFar);

                List<SpawnEvent> batch = new List<SpawnEvent>();
                foreach(BarricadeDirection direction in segment.directions)
                {
                    for(int i = 0; i < remainingInThisDirection; i++)
                    {
                        batch.Add(new SpawnEvent { type = segment.type, direction = direction });
                    }
                }
                spawnQueue.Enqueue(batch);
            }
        }

        aliveCount = 0;
        spawnTimer = 0f;
        IsCleared = false;
        spawningStarted = true;
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

    private void Update()
    {
        if (!spawningStarted || IsCleared) return;
        if (player == null) return; // 플레이어가 아직 준비 안됐으면 스폰 대기

        if(spawnQueue.Count > 0)
        {
            spawnTimer -= Time.deltaTime;
            if(spawnTimer <= 0f) // spawnTimer 가 0f 로 시작해서 첫 프레임에 바로 스폰될 수 있고, player 주입 시점과 경쟁할 수도 있음
            {
                SpawnEnemy();
                spawnTimer = waveDefinition.SpawnInterval;
            }
        }
        else if(aliveCount <= 0)
        {
            IsCleared = true;
        }
    }

    private void SpawnEnemy()
    {
        List<SpawnEvent> batch = spawnQueue.Dequeue();
        foreach(SpawnEvent spawnEvent in batch)
        {
            Transform spawnPoint = GetSpawnPoint(spawnEvent.direction);
            EnemyMovement enemy = EnemyPool.Instance.Get(spawnEvent.type, spawnPoint.position, spawnPoint.rotation);
            enemy.SetTarget(player);

            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            health.Initialize(timeOfDayModifier.EnemyStrengthMultiplier, OnEnemyDeath);
            
            aliveCount++;
        }
    }

    private Transform GetSpawnPoint(BarricadeDirection direction)
    {
        return spawnPoints[(int)direction];
    }

    private void OnEnemyDeath()
    {
        aliveCount--;
    }
}
