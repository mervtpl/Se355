using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public GameObject flyingEnemyPrefab;
    public Transform[] spawnPoints;
    public float timeBetweenLevels = 2f;
    public int baseEnemyCount = 3;
    public int enemyIncreasePerLevel = 2;
    public int baseEnemyHealth = 3;
    public float baseEnemyMoveSpeed = 2f;
    public float enemyMoveSpeedIncreasePerLevel = 0.15f;
    public Vector2 randomSpawnMin = new Vector2(-12f, -7f);
    public Vector2 randomSpawnMax = new Vector2(12f, 7f);
    public float minSpawnDistanceFromPlayer = 4f;

    public int currentLevel = 1;
    public int enemiesAlive;

    private readonly List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
    private Transform playerTarget;
    private bool levelTransitionInProgress;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance?.RegisterLevelManager(this);
        TryFindPlayer();
        StartLevel(currentLevel);
    }

    public void NotifyEnemyKilled(EnemyHealth enemy)
    {
        activeEnemies.Remove(enemy);
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        GameManager.Instance?.OnEnemyCountChanged(enemiesAlive);

        if (enemiesAlive == 0 && !levelTransitionInProgress)
        {
            StartCoroutine(AdvanceLevelRoutine());
        }
    }

    private void StartLevel(int level)
    {
        CleanupEnemyList();
        currentLevel = Mathf.Max(1, level);
        int enemyCount = baseEnemyCount + (currentLevel - 1) * enemyIncreasePerLevel;
        enemiesAlive = enemyCount;
        levelTransitionInProgress = false;

        GameManager.Instance?.OnLevelStarted(currentLevel, enemiesAlive);

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy(i);
        }
    }

    private IEnumerator AdvanceLevelRoutine()
    {
        levelTransitionInProgress = true;
        GameManager.Instance?.OnLevelComplete();

        yield return new WaitForSeconds(timeBetweenLevels);

        if (GameManager.Instance == null || !GameManager.Instance.IsGameOver)
        {
            StartLevel(currentLevel + 1);
        }
    }

    private void SpawnEnemy(int index)
    {
        if (flyingEnemyPrefab == null)
        {
            enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
            GameManager.Instance?.OnEnemyCountChanged(enemiesAlive);
            return;
        }

        TryFindPlayer();
        Vector2 spawnPosition = GetSpawnPosition(index);
        GameObject enemyObject = Instantiate(flyingEnemyPrefab, spawnPosition, Quaternion.identity);
        enemyObject.SetActive(true);

        EnemyHealth health = enemyObject.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.SetMaxHealth(baseEnemyHealth + currentLevel - 1);
            activeEnemies.Add(health);
        }

        FlyingEnemyMovement movement = enemyObject.GetComponent<FlyingEnemyMovement>();
        if (movement != null)
        {
            movement.playerTarget = playerTarget;
            movement.moveSpeed = baseEnemyMoveSpeed + (currentLevel - 1) * enemyMoveSpeedIncreasePerLevel;
        }
    }

    private Vector2 GetSpawnPosition(int index)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[index % spawnPoints.Length];
            if (spawnPoint != null)
            {
                return spawnPoint.position;
            }
        }

        Vector2 fallback = Vector2.zero;
        for (int attempt = 0; attempt < 20; attempt++)
        {
            fallback = new Vector2(Random.Range(randomSpawnMin.x, randomSpawnMax.x), Random.Range(randomSpawnMin.y, randomSpawnMax.y));
            if (playerTarget == null || Vector2.Distance(fallback, playerTarget.position) >= minSpawnDistanceFromPlayer)
            {
                break;
            }
        }

        return fallback;
    }

    private void TryFindPlayer()
    {
        if (playerTarget != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }
    }

    private void CleanupEnemyList()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }
}
