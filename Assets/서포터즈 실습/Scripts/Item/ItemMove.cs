using UnityEngine;

public class ItemMove : MonoBehaviour
{
    void Update()
    {
        // GameManager에서 현재 게임 속도를 가져옴
        float speed = 10f;
        if (GameManager.Instance != null)
        {
            speed = GameManager.Instance.gameSpeed;
        }

        // 장애물, 바닥과 동일한 속도로 뒤로 이동
        transform.Translate(Vector3.back * speed * Time.deltaTime);

        // 플레이어를 지나쳐 화면 밖으로 벗어나면 메모리 해제를 위해 파괴
        if (transform.position.z < -10f)
        {
            Destroy(gameObject);
        }
    }
}