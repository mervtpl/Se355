using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
	public GameObject enemyPrefab;
	public PlayerCharacter player;

	public float spawnInterval = 3f;
	public int maxEnemies = 5;
	public float spawnRadius = 6f;
	public float enemySpeed = 2f;
	public float damage = 10f;

	private List<GameObject> enemies = new List<GameObject>();

	void Start()
	{
		InvokeRepeating("SpawnEnemy", 1f, spawnInterval);
	}

	void Update()
	{
		if (player == null)
			return;

		for (int i = enemies.Count - 1; i >= 0; i--)
		{
			if (enemies[i] == null)
			{
				enemies.RemoveAt(i);
				continue;
			}

			GameObject enemy = enemies[i];

			enemy.transform.position = Vector2.MoveTowards(
				enemy.transform.position,
				player.transform.position,
				enemySpeed * Time.deltaTime
			);

			float distance = Vector2.Distance(
				enemy.transform.position,
				player.transform.position
			);

			if (distance < 0.7f)
			{
				player.TakeDamage(damage);
			}
		}
	}

	void SpawnEnemy()
	{
		if (enemies.Count >= maxEnemies || player == null)
			return;

		Vector2 randomDirection = Random.insideUnitCircle.normalized;
		Vector2 spawnPosition =
			(Vector2)player.transform.position +
			randomDirection * spawnRadius;

		GameObject newEnemy = Instantiate(
			enemyPrefab,
			spawnPosition,
			Quaternion.identity
		);

		enemies.Add(newEnemy);
	}
}