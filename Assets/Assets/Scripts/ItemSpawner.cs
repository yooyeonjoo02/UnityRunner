using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Item Settings")]
    public GameObject[] itemPrefabs; // 4가지 아이템 프리팹을 할당할 배열
    
    [Header("Spawn Settings")]
    public float spawnDistance = 150f; // 장애물(30f)보다 더 드문드문 나오도록 간격 설정
    public float spawnZ = 200f;        // 장애물과 동일한 생성 거리
    
    public float defaultY = 1f;        // 기본 바닥 생성 높이
    public float floatingY = 4.5f;     // 장애물이 있을 때 점프해서 먹을 수 있는 공중 높이

    private float[] lanes;
    private float distanceAccumulator = 0f; 


    public static ItemSpawner Instance; // (ItemSpawner에서 참고)
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            lanes = GameManager.Instance.lanes;
        }
        else
        {
            lanes = new float[] { -3f, 0f, 3f };
        }
        spawnZ = GameManager.Instance.spawnZ;
        // 시작하자마자 하나 생성되지 않게 하려면 0f, 바로 생성하려면 spawnDistance로 초기화
        distanceAccumulator = 0f; 
    }

    // LateUpdate를 지우고, ObstacleSpawner가 호출하는 함수 생성
    public void CheckAndSpawn(int obstacleMask, float addedDistance)
    {
        // 장애물이 스폰된 간격(예: 30f)에 맞게 '더해'줌으로써 소수점 오차 발생 제거
        distanceAccumulator += addedDistance;

        if (distanceAccumulator >= spawnDistance)
        {
            SpawnItem(obstacleMask); // 전달받은 마스크를 그대로 사용
            distanceAccumulator -= spawnDistance; 
        }
    }

    private void SpawnItem(int currentObstacleMask) // 매개변수로 마스크를 받음
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogWarning("ItemPrefabs 배열이 비어있습니다!");
            return;
        }

        // 1. 랜덤 라인 및 랜덤 아이템 프리팹 선택
        int randomLane = Random.Range(0, lanes.Length);
        int randomItem = Random.Range(0, itemPrefabs.Length);
        
        Vector3 spawnPosition = new Vector3(lanes[randomLane], defaultY, spawnZ);
        bool isObstaclePresent = false;

        int itemLaneBit = 1 << randomLane;

        // 2. 매개변수로 넘어온 이번 프레임의 확실한 마스크와 비교
        if ((currentObstacleMask & itemLaneBit) != 0)
        {
            isObstaclePresent = true;
        }

        // 3. 장애물이 있다면 아이템의 Y축 좌표를 상승
        if (isObstaclePresent)
        {
            spawnPosition.y = floatingY;
            Debug.Log("비트마스크 판정: 장애물과 겹침 감지");
        }

        // 4. 아이템 최종 생성
        Instantiate(itemPrefabs[randomItem], spawnPosition, Quaternion.identity);
    }
}