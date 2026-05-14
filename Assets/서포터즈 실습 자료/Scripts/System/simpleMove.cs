using UnityEngine;

public class SimpleMove : MonoBehaviour
{
    private float moveSpeed = 5.0f;
    private bool isGrounded = true;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Jump();
        // 1. 키보드 입력 받기 (WASD, 화살표 자동 매핑)
        float h = Input.GetAxis("Horizontal"); // 좌우 (-1.0 ~ 1.0)
        float v = Input.GetAxis("Vertical");   // 상하 (-1.0 ~ 1.0)

        // 2. 이동 방향 계산 (X축과 Z축 기준)
        Vector3 moveDirection = new Vector3(h, 0, v);

        // 3. 위치 업데이트
        // 방향 * 속도 * 프레임 보정값
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isGrounded = false;
            rb.AddForce(Vector3.up * 5, ForceMode.Impulse);
        }
    }
}