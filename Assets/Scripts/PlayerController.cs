using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;     // 라인 이동(변경) 속도
    public float jumpForce = 8.5f;   // 점프 높이
    
    private float baseGameSpeed = 10f; // 기준이 되는 초기 게임 스피드

    private Rigidbody rb;
    public bool isGrounded;

    private Animator animator; // Animation Controller

    // 3라인 이동 관련 변수
    private float[] lanes;
    private int currentLane = 1; // 0: 좌측, 1: 중앙, 2: 우측

    // 아이템 상태 관련 변수들
    private float originalMoveSpeed;
    private bool hasShield = false;
    private bool isGhostMode = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        originalMoveSpeed = moveSpeed;

        // GameManager로부터 라인 정보 초기화
        if (GameManager.Instance != null)
        {
            lanes = GameManager.Instance.lanes;
            baseGameSpeed = GameManager.Instance.gameSpeed;
        }
        else
        {
            // 예외 처리용 기본값
            lanes = new float[] { -3f, 0f, 3f }; 
        }

        // 기본 중력을 끄고 스크립트에서 커스텀 중력을 통제합니다.
        rb.useGravity = false;
    }

    // 물리 연산 프레임에서 커스텀 중력 계산 및 적용
    void FixedUpdate()
    {
        float speedRatio = 1f;
        if (GameManager.Instance != null)
        {
            speedRatio = GameManager.Instance.gameSpeed / baseGameSpeed;
        }

        // 중력 = 기본 중력 * (속도 배율의 제곱)
        // 체공 시간은 짧아지고 높이는 유지되도록 제곱(Pow)을 사용
        Vector3 customGravity = Physics.gravity * speedRatio * speedRatio;
        rb.AddForce(customGravity, ForceMode.Acceleration);
    }

    void Update()
    {
        SyncSpeed(); // 속도 동기화
        Move();
        Jump();
        
        if(transform.position.y < 0f) // 속도가 너무 빨라져서 바닥을 뚫는 예외 경우 제어하기 위함
        {
            transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
            isGrounded = true;
        }
    }

    private void SyncSpeed()
    {
        if (GameManager.Instance != null)
        {
            // 1. GameManager의 속도 비율 계산 (기본 속도 대비)
            // 기본 속도를 10이라고 가정할 때의 비율
            float speedRatio = GameManager.Instance.gameSpeed / 10f;

            // 2. 라인 이동 속도 동기화 
            // 기본 moveSpeed에 비율을 곱해 이동 속도 상승
            moveSpeed = originalMoveSpeed * speedRatio;

            // 3. 애니메이션 속도 동기화 
            // 달리기나 점프 애니메이션이 게임 템포에 맞게 빨라짐
            if (animator != null)
            {
                animator.speed = speedRatio;
            }
        }
    }

    void Move()
    {
        // 한 번 누를 때마다 한 라인씩 이동
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (currentLane > 0)
            {
                currentLane--; // 왼쪽 라인으로 이동
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (currentLane < lanes.Length - 1)
            {
                currentLane++; // 오른쪽 라인으로 이동
            }
        }

        // 목표 라인으로 연속적으로 X좌표 이동 (Y와 Z는 현재 위치 유지)
        float targetX = lanes[currentLane];
        float newX = Mathf.MoveTowards(transform.position.x, targetX, moveSpeed * Time.deltaTime);
        
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Debug.Log("jumpping!");
            
            float speedRatio = 1f;
            if (GameManager.Instance != null)
            {
                speedRatio = GameManager.Instance.gameSpeed / baseGameSpeed;
            }

            // 점프력 = 기존 점프력 * (속도 배율)
            // 중력이 제곱으로 강해졌으므로, 초기 점프 속도를 배율만큼 높여주어야 원래 높이까지 도달함
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Y축 속도 초기화 (안정성 확보)
            rb.AddForce(Vector3.up * (jumpForce * speedRatio), ForceMode.Impulse);
            
            isGrounded = false;
            animator.SetTrigger("isJumping");
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {

            if (isGhostMode)
            {
                // 아이템 먹고 고스트 모드일 경우 충돌을 무시하고 그대로 통과/진행
                return; 
            }

            if (hasShield)
            {
                Debug.Log("쉴드로 1회 방어!");
                hasShield = false; // 쉴드 소모
                
                // 부딪힌 장애물을 파괴
                Destroy(collision.gameObject); 
                return;
            }


            Debug.Log("hitting an obstacle!");
            FindObjectOfType<GameManager>().GameOver();
        }
    }

    #region 게임 아이템 효과 적용 API
    // 1. 쉴드 적용
    public void AddShield()
    {
        hasShield = true;
    }

    // 2. 속도 감소 적용
    public void ApplySlowEffect(float multiplier, float duration)
    {
        StartCoroutine(SpeedChangeRoutine(multiplier, duration));
    }

    // 3. 속도 증가 적용
    public void ApplyFastEffect(float multiplier, float duration)
    {
        StartCoroutine(SpeedChangeRoutine(multiplier, duration));
        
        // GameManager의 시간 가속도 함께 호출
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyTimeScaleEffect(multiplier, duration);
        }
    }

    // 속도 변경 코루틴 (공통)
    private System.Collections.IEnumerator SpeedChangeRoutine(float multiplier, float duration)
    {
        moveSpeed = originalMoveSpeed * multiplier;
        yield return new WaitForSecondsRealtime(duration); // TimeScale 변경에 영향받지 않도록 Realtime 사용
        moveSpeed = originalMoveSpeed;
    }

    // 4. 고스트 모드 적용
    public void ActivateGhostMode(float duration)
    {
        if (!isGhostMode) 
        {
            StartCoroutine(GhostModeRoutine(duration));
        }
    }

    private System.Collections.IEnumerator GhostModeRoutine(float duration)
    {
        isGhostMode = true;
        
        // 맵에 있는 모든 장애물을 찾아서 반투명하게 처리
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obs in obstacles)
        {
            Renderer rend = obs.GetComponent<Renderer>();
            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = 0.3f; // 알파값을 낮춰 반투명하게 (단, Material이 Transparent여야함)
                rend.material.color = c;
            }
        }

        yield return new WaitForSecondsRealtime(duration);

        // 지속시간 종료 후 원래 투명도로 복구
        isGhostMode = false;
        
        // 새로 생성된 장애물도 있을 수 있으므로 다시 Find 수행
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obs in obstacles)
        {
            Renderer rend = obs.GetComponent<Renderer>();
            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = 1.0f; // 알파값 원상복구
                rend.material.color = c;
            }
        }
    }
    #endregion
}