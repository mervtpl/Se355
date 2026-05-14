using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public float spawnInterval = 3f;
    public int maxAliveEnemies = 10;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                continue;
            }

            TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        CleanupList();

        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || spawnPoints == null || spawnPoints.Length == 0)
        {
            return;
        }

        if (aliveEnemies.Count >= maxAliveEnemies)
        {
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        if (spawnPoint == null || prefab == null)
        {
            return;
        }

        GameObject spawnedEnemy = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        spawnedEnemy.SetActive(true);
        aliveEnemies.Add(spawnedEnemy);
    }

    private void CleanupList()
    {
        aliveEnemies.RemoveAll(enemy => enemy == null);
    }
}
