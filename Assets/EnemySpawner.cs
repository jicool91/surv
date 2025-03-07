using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform player;
    public float spawnInterval = 2f;
    private float timer;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnEnemy();
            timer = spawnInterval;
        }
    }

    void SpawnEnemy()
    {
        Vector2 spawnPos = (Vector2)player.position + Random.insideUnitCircle.normalized * 5f;
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}