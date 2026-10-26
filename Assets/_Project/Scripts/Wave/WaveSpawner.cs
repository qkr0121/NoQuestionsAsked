using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveSpawner : MonoBehaviour
{
    private class SpawnEvent
    {
        public EnemyType type;
        public BarricadeDirection direction;
    }

    // 한 틱마다 동시에 나갈 좀비 묶음 - 구간의 첫 묶음만 대기/경고를 가짐
    private class SpawnBatch
    {
        public List<SpawnEvent> events = new List<SpawnEvent>();
        public float delayBefore;   // 0이면 기본 간격
        public bool showWarning;
        public bool waitForClear;
    }

    [SerializeField] private Transform[] spawnPoints; // 순서: North, East, South, West(BarricadeDirection 과 일치)
    [SerializeField] private float spawnOffset = 1.5f;   // 변에서 안쪽/바깥쪽(깊이) 방향으로 흩뿌리는 범위 - 덤불 띠 안에서 시작 깊이를 조금씩 다르게 함
    [SerializeField] private float edgeSpread = 22f;     // 변을 따라 좌우로 흩뿌리는 범위 - 한 곳이 아니라 맵 끝 전체의 덤불 사이에서 한 마리씩 나오게 함
    [SerializeField] private float clearToWarningDelay = 1.5f;   // 마지막 좀비를 잡은 뒤 경고·대기를 시작하기까지 숨 돌리는 시간

    private Transform player;
    private WaveDefinition waveDefinition;

    private Queue<SpawnBatch> spawnQueue;
    private int aliveCount;
    private float spawnTimer;
    private bool spawningStarted;
    private bool waitingForClear;   // 필드가 빌 때까지 다음 묶음의 대기를 시작하지 않는 상태
    private float clearTimer;       // 필드가 빈 뒤 지난 시간 - clearToWarningDelay 만큼 지나야 경고·대기를 시작함

    public event Action<float> OnWarning;   // 인자: 경고 지속 시간(초)

    public bool IsCleared { get; private set; }
    public bool CanExit => !spawningStarted || IsCleared;
    // 아직 웨이브를 시작 안했거나(웨이브 미선택 상태), 이미 클리어했으면 나갈 수 있음

    public void BeginWave(WaveDefinition definition)
    {
        waveDefinition = definition;

        spawnQueue = new Queue<SpawnBatch>();
        foreach (WaveDefinition.EnemySpawnSegment segment in definition.SpawnSegments)
        {
            int tickCount = Mathf.CeilToInt((float)segment.count / segment.simultaneousCount); // 이 구간이 몇 번의 틱으로 나뉘는지

            for(int tick = 0; tick < tickCount; tick++)
            {
                int spawnedSoFar = tick * segment.simultaneousCount;
                int remainingInThisDirection = Mathf.Min(segment.simultaneousCount, segment.count - spawnedSoFar);

                SpawnBatch batch = new SpawnBatch();
                if (tick == 0)
                {
                    batch.delayBefore = segment.delayBefore;
                    batch.showWarning = segment.showWarning;
                    batch.waitForClear = segment.waitForClear;
                }

                foreach(BarricadeDirection direction in segment.directions)
                {
                    for(int i = 0; i < remainingInThisDirection; i++)
                    {
                        batch.events.Add(new SpawnEvent { type = segment.type, direction = direction });
                    }
                }
                spawnQueue.Enqueue(batch);
            }
        }

        aliveCount = 0;
        IsCleared = false;
        waitingForClear = false;
        clearTimer = 0f;
        spawningStarted = true;
        ScheduleNextBatch(0f);   // 첫 묶음은 기본 대기 없이 바로, 대기가 지정돼 있으면 그만큼 뒤에
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

    private void ScheduleNextBatch(float defaultWait)
    {
        if (spawnQueue.Count == 0) return;

        SpawnBatch next = spawnQueue.Peek();
        if (next.waitForClear)
        {
            waitingForClear = true;   // 필드의 좀비를 모두 잡은 뒤에 대기와 경고를 시작함
            return;
        }
        StartWait(next, defaultWait);
    }

    // 다음 묶음이 나가기까지 대기 시간을 정하고, 경고가 필요하면 알림
    private void StartWait(SpawnBatch next, float defaultWait)
    {
        spawnTimer = next.delayBefore > 0f ? next.delayBefore : defaultWait;
        if (next.showWarning) OnWarning?.Invoke(spawnTimer);
    }

    private void Update()
    {
        if (!spawningStarted || IsCleared) return;
        if (player == null) return; // 플레이어가 아직 준비 안됐으면 스폰 대기

        if(spawnQueue.Count > 0)
        {
            if (waitingForClear)
            {
                if (aliveCount > 0)
                {
                    clearTimer = 0f;   // 아직 필드에 좀비가 남아 있음
                    return;
                }

                clearTimer += Time.deltaTime;
                if (clearTimer < clearToWarningDelay) return;   // 마지막 좀비를 잡은 직후 잠깐 쉼

                clearTimer = 0f;
                waitingForClear = false;
                StartWait(spawnQueue.Peek(), 0f);
                return;
            }

            spawnTimer -= Time.deltaTime;
            if(spawnTimer <= 0f) // spawnTimer 가 0f 로 시작해서 첫 프레임에 바로 스폰될 수 있고, player 주입 시점과 경쟁할 수도 있음
            {
                SpawnEnemy();
                ScheduleNextBatch(waveDefinition.SpawnInterval);
            }
        }
        else if(aliveCount <= 0)
        {
            IsCleared = true;
        }
    }

    private void SpawnEnemy()
    {
        SpawnBatch batch = spawnQueue.Dequeue();
        foreach(SpawnEvent spawnEvent in batch.events)
        {
            Transform spawnPoint = GetSpawnPoint(spawnEvent.direction);

            // 북·남은 x(가로), 동·서는 z 방향이 변을 따라가는 축 - 그 축으로 넓게, 나머지(깊이) 축으로는 조금만 흩뿌림
            bool alongX = spawnEvent.direction == BarricadeDirection.North || spawnEvent.direction == BarricadeDirection.South;
            Vector3 along = alongX ? Vector3.right : Vector3.forward;
            Vector3 depth = alongX ? Vector3.forward : Vector3.right;
            Vector3 offset = along * Random.Range(-edgeSpread, edgeSpread) + depth * Random.Range(-spawnOffset, spawnOffset);

            EnemyMovement enemy = EnemyPool.Instance.Get(spawnEvent.type, spawnPoint.position + offset, spawnPoint.rotation);
            enemy.SetTarget(player);

            // 시간대 배율은 하루 단위 단순화로 제거 - 필요하면 웨이브 단위 배율로 복구
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            health.Initialize(1f, OnEnemyDeath);

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
