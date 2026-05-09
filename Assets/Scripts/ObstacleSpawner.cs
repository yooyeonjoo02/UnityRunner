using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;

    public float spawnInterval = 2f;
    public float spawnZ = 200f;

    public float[] lanes = { -3f, 0f, 3f };

    private void Start()
    {
        lanes = GameManager.Instance.lanes;
        InvokeRepeating(nameof(SpawnObstacle), 1f, spawnInterval);
    }

    private void SpawnObstacle()
    {
        int randomLane = Random.Range(0, lanes.Length);

        Vector3 spawnPosition = new Vector3(lanes[randomLane], 1f, spawnZ);

        Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
    }
}