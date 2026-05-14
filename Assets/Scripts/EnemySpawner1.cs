using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;
    public int maxAliveEnemies = 8;
    public float spawnRadius = 0.35f;
    public bool spawnImmediately = true;
    public Transform target;

    List<EnemyFollower> aliveEnemies = new List<EnemyFollower>();
    Coroutine spawnRoutine;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void OnEnable()
    {
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    public void SpawnEnemy()
    {
        CompactAliveList();

        if (enemyPrefab == null || aliveEnemies.Count >= maxAliveEnemies)
        {
            return;
        }

        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        GameObject enemyObject = Instantiate(enemyPrefab, transform.position + (Vector3)offset, Quaternion.identity);
        enemyObject.name = enemyPrefab.name;

        EnemyFollower follower = enemyObject.GetComponent<EnemyFollower>();
        if (follower != null)
        {
            follower.SetTarget(GetTarget());
            aliveEnemies.Add(follower);
        }
    }

    IEnumerator SpawnLoop()
    {
        if (spawnImmediately)
        {
            SpawnEnemy();
        }

        while (enabled)
        {
            yield return new WaitForSeconds(Mathf.Max(0.1f, spawnInterval));
            SpawnEnemy();
        }
    }

    void CompactAliveList()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }

    Transform GetTarget()
    {
        if (target != null)
        {
            return target;
        }

        if (PlayerController.Current != null)
        {
            return PlayerController.Current;
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();
        return player != null ? player.transform : null;
    }
}
