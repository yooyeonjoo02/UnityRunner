using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    
    // 시간(초) 기준이 아닌 물리적인 '거리' 기준으로 변경
    public float spawnDistance = 30f; // 장애물 간의 유지될 일정한 간격 (거리)
    public float spawnZ = 200f;       // 생성 Z 위치 (충분히 먼 곳)

    public float[] lanes = { -3f, 0f, 3f };
    
    // 속도에 따라 이동한 거리를 누적할 변수
    private float distanceAccumulator = 0f; 

    // 비트마스크로 생성된 마지막으로 생성된 위치 저장 (ItemSpawner에서 참고)
    public int lastSpawnMask = 0;

    // 장애물 2개 등장 확률 계산용 변수들
    private float elapsedTime = 0f;
    private float doubleSpawnProb = 0.01f; // 시작 확률 1%
    private float thresholdTime = 40f; // 임계 시간
    private float thresholdProb = 0.9f; // 임계 확률


    public static ObstacleSpawner Instance; // (ItemSpawner에서 참고)
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
        spawnZ = GameManager.Instance.spawnZ;
        // 처음 시작하자마자 하나 생성하기 위해 누적기를 목표치로 초기화
        distanceAccumulator = spawnDistance; 
    }

    private void Update()
    {
        // 1. 매 프레임마다 GameManager의 속도를 가져옴
        float currentSpeed = 10f;
        if (GameManager.Instance != null)
        {
            currentSpeed = GameManager.Instance.gameSpeed;
        }

        elapsedTime += Time.deltaTime;

        // 2. 속도에 비례하여 이동한 '거리'를 누적 (거리 = 속도 * 시간)
        distanceAccumulator += currentSpeed * Time.deltaTime;

        // 3. 누적된 거리가 설정한 간격(spawnDistance)에 도달하면 장애물 생성
        if (distanceAccumulator >= spawnDistance)
        {
            SpawnObstacle();
            
            // 누적기를 0으로 초기화하지 않고 초과분만큼 빼주면 오차 없이 더욱 정확하게 간격이 유지됩니다.
            distanceAccumulator -= spawnDistance; 
        }
    }

    private void SpawnObstacle()
    {
        // 1. 마스크 초기화
        lastSpawnMask = 0;

        // 2. 확률 계산 (지수 성장: 곱연산 방식)
        // threshold초일 때 thresholdProb(thresholdProb%)에 도달하도록 설정: P(t) = 0.01 * (thresholdProb*100)^(t / thresholdTime)
        if (doubleSpawnProb < thresholdProb)
        {
            doubleSpawnProb = 0.01f * Mathf.Pow(thresholdProb*100, elapsedTime / thresholdTime);
        }
        else
        {
            // 임계 이상이면 계산을 멈추고 임계값으로 고정
            doubleSpawnProb = thresholdProb; 
        }

        // 3. 2개 생성 여부 판정
        bool spawnDouble = Random.value <= doubleSpawnProb;

        if (spawnDouble)
        {
            // 중복되지 않는 2개의 랜덤 라인 선택
            int lane1 = Random.Range(0, lanes.Length);
            int lane2;
            do
            {
                lane2 = Random.Range(0, lanes.Length);
            } while (lane1 == lane2); // 같으면 다시 뽑기

            // 2개 라인 모두 비트를 켬 (예: 0번과 2번이면 101)
            lastSpawnMask = (1 << lane1) | (1 << lane2);

            // 실제 오브젝트 2개 생성
            Instantiate(obstaclePrefab, new Vector3(lanes[lane1], 1f, spawnZ), Quaternion.identity);
            Instantiate(obstaclePrefab, new Vector3(lanes[lane2], 1f, spawnZ), Quaternion.identity);
        }
        else
        {
            int lane1 = Random.Range(0, lanes.Length);
            lastSpawnMask = (1 << lane1); // 선택된 라인의 비트를 기록 (예: 1번 라인이면 010)
            Instantiate(obstaclePrefab, new Vector3(lanes[lane1], 1f, spawnZ), Quaternion.identity);
        }

        // 장애물 스폰을 완료한 즉시, 아이템 스포너에게 동기화 지시
        if (ItemSpawner.Instance != null)
        {
            // 방금 만든 마스크와 기준 거리(30f)를 전달
            ItemSpawner.Instance.CheckAndSpawn(lastSpawnMask, spawnDistance);
        }
    }
}