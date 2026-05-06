using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;     // 이동 속도
    public float jumpForce = 3.5f;     // 점프 높이

    private Rigidbody rb;
    private bool isGrounded;

    private Animator animator; // Animation Controller

    // 아이템 상태 관련 변수들
    private float originalMoveSpeed;
    private bool hasShield = false;
    private bool isGhostMode = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        originalMoveSpeed = moveSpeed;
    }

    void Update()
    {
        Move();
        Jump();
    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal"); // A/D or 좌 우
        //Vector3 move = new Vector3(h * moveSpeed, 0, 0);
        //transform.Translate(move * Time.deltaTime);
        rb.linearVelocity = new Vector3(h * moveSpeed, rb.linearVelocity.y, 0);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Debug.Log("jumpping!");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("isJumping");
            isGrounded = false;
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