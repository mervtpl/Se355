using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemySpawner : MonoBehaviour
{
    public GameObject flyingEnemyPrefab;
    public Transform playerTarget;
    public Transform[] spawnPoints;
    public float spawnInterval = 1f;
    public bool spawnOnStart = true;
    public int maxAliveEnemies = 0;
    public Vector2 randomSpawnMin = new Vector2(-8f, -4f);
    public Vector2 randomSpawnMax = new Vector2(8f, 4f);

    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();
    private Coroutine spawnRoutine;
    private int nextSpawnPointIndex;
    private float nextPlayerSearchTime;

    private void Start()
    {
        if (spawnOnStart)
        {
            StartSpawning();
        }
    }

    private void Update()
    {
        CleanupDestroyedEnemies();

        if (playerTarget == null && Time.time >= nextPlayerSearchTime)
        {
            TryFindPlayer();
            nextPlayerSearchTime = Time.time + 1f;
        }
    }

    public void StartSpawning()
    {
        if (spawnRoutine != null)
        {
            return;
        }

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnRoutine == null)
        {
            return;
        }

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    public void SpawnEnemyNow()
    {
        CleanupDestroyedEnemies();
        TryFindPlayer();

        if (flyingEnemyPrefab == null)
        {
            return;
        }

        if (maxAliveEnemies > 0 && spawnedEnemies.Count >= maxAliveEnemies)
        {
            return;
        }

        Vector2 spawnPosition = GetSpawnPosition();
        GameObject enemyObject = Instantiate(flyingEnemyPrefab, spawnPosition, Quaternion.identity, transform);
        spawnedEnemies.Add(enemyObject);

        FlyingEnemyMovement movement = enemyObject.GetComponent<FlyingEnemyMovement>();
        if (movement != null && playerTarget != null)
        {
            movement.playerTarget = playerTarget;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnEnemyNow();
        }
    }

    private Vector2 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Transform spawnPoint = spawnPoints[nextSpawnPointIndex % spawnPoints.Length];
                nextSpawnPointIndex++;

                if (spawnPoint != null)
                {
                    return spawnPoint.position;
                }
            }
        }

        float x = Random.Range(randomSpawnMin.x, randomSpawnMax.x);
        float y = Random.Range(randomSpawnMin.y, randomSpawnMax.y);
        return new Vector2(x, y);
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

    private void CleanupDestroyedEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }
}
