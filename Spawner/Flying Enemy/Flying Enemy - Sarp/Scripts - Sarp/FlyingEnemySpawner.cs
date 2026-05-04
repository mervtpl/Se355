using UnityEngine;

public class FlyingEnemySpawner : MonoBehaviour
{
    [Header("References")]
    public Transform playerTarget;
    public GameObject enemyTemplate;

    [Header("Spawn Settings")]
    public Vector2 spawnRadius = new Vector2(9f, 12f);
    public float spawnInterval = 1.75f;
    public int maxEnemyCount = 8;
    public float enemyFlightHeight = 1.8f;
    public float minSpawnSeparation = 2f;

    private float nextSpawnTime;
    private int aliveEnemyCount;

    private void Start()
    {
        nextSpawnTime = Time.time + 0.5f;
    }

    private void Update()
    {
        if (playerTarget == null || aliveEnemyCount >= maxEnemyCount || Time.time < nextSpawnTime)
        {
            return;
        }

        if (playerTarget.TryGetComponent<PlayerHealthFeedback>(out var playerHealth) && playerHealth.IsInfected)
        {
            return;
        }

        SpawnEnemy();
        nextSpawnTime = Time.time + spawnInterval;
    }

    private void SpawnEnemy()
    {
        var spawnPosition = GetSpawnPosition();

        var enemyRoot = enemyTemplate != null
            ? Instantiate(enemyTemplate, spawnPosition, Quaternion.identity)
            : new GameObject($"Flying Enemy {aliveEnemyCount + 1}");
        enemyRoot.name = $"Flying Enemy {aliveEnemyCount + 1}";
        enemyRoot.transform.position = spawnPosition;
        enemyRoot.SetActive(true);

        var enemy = enemyRoot.GetComponent<FlyingEnemy>();
        if (enemy == null)
        {
            enemy = enemyRoot.AddComponent<FlyingEnemy>();
        }

        enemy.playerTarget = playerTarget;
        enemy.flightHeight = enemyFlightHeight;
        enemy.onDestroyed += HandleEnemyDestroyed;

        if (enemyTemplate == null)
        {
            FlyingEnemyDemoBootstrap.EnsureVirusVisual(enemyRoot.transform, enemy.EnemyColor);
        }

        aliveEnemyCount++;
    }

    private Vector3 GetSpawnPosition()
    {
        const int maxAttempts = 12;

        for (var i = 0; i < maxAttempts; i++)
        {
            var spawnPosition = CreateRandomSpawnPosition();
            if (FlyingEnemy.IsSpawnPointClear(spawnPosition, minSpawnSeparation))
            {
                return spawnPosition;
            }
        }

        return CreateRandomSpawnPosition();
    }

    private Vector3 CreateRandomSpawnPosition()
    {
        var spawnDirection = Random.insideUnitCircle.normalized;
        if (spawnDirection == Vector2.zero)
        {
            spawnDirection = Vector2.right;
        }

        var radius = Random.Range(spawnRadius.x, spawnRadius.y);
        var spawnOffset = spawnDirection * radius;
        return new Vector3(spawnOffset.x, enemyFlightHeight, spawnOffset.y);
    }

    private void HandleEnemyDestroyed()
    {
        aliveEnemyCount = Mathf.Max(0, aliveEnemyCount - 1);
    }
}
