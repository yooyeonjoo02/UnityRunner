using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;     // �¿� �̵� �ӵ�
    public float jumpForce = 7f;     // ���� ��

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Move();
        Jump();
    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal"); // A/D or �� ��
        //Vector3 move = new Vector3(h * moveSpeed, 0, 0);
        //transform.Translate(move * Time.deltaTime);
        rb.linearVelocity = new Vector3(h * moveSpeed, rb.linearVelocity.y, 0);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
            Debug.Log("���� ����!");
            FindObjectOfType<GameManager>().GameOver();
        }
    }
}