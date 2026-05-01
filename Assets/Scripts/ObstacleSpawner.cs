using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;

    public float spawnInterval = 2f;
    public float spawnZ = 30f;

    public float[] laneX = { -3f, 0f, 3f };

    private void Start()
    {
        InvokeRepeating(nameof(SpawnObstacle), 1f, spawnInterval);
    }

    private void SpawnObstacle()
    {
        int randomLane = Random.Range(0, laneX.Length);

        Vector3 spawnPosition = new Vector3(laneX[randomLane], 1f, spawnZ);

        Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
    }
}