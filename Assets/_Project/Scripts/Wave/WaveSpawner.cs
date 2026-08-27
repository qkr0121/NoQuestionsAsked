using System.Threading;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    private Transform player;
    private WaveDefinition waveDefinition;
    private TimeOfDayModifier timeOfDayModifier;

    private int totalToSpawn;
    private int spawnedCount;
    private int aliveCount;
    private float spawnTimer;
    private bool spawningStarted;

    public bool IsCleared { get; private set; }
    public bool CanExit => !spawningStarted || IsCleared;

    public void BeginWave(WaveDefinition definition, TimeOfDayModifier modifier)
    {
        waveDefinition = definition;
        timeOfDayModifier = modifier;

        totalToSpawn = Mathf.Max(1, Mathf.RoundToInt(definition.TotalEnemyCount * modifier.SpawnCountMultiplier));
        spawnedCount = 0;
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

        if(spawnedCount < totalToSpawn)
        {
            spawnTimer -= Time.deltaTime;
            if(spawnTimer <= 0f)
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
        if (spawnPoints.Length == 0) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        EnemyMovement enemy = EnemyPool.Instance.Get(spawnPoint.position, spawnPoint.rotation);
        enemy.SetTarget(player);

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        health.Initialize(timeOfDayModifier.EnemyStrengthMultiplier, OnEnemyDeath);

        spawnedCount++;
        aliveCount++;
    }

    private void OnEnemyDeath()
    {
        aliveCount--;
    }
}
