using UnityEngine;

public class ObstacleMove : MonoBehaviour
{
    // public float moveSpeed = 10f;

    void Update()
    {
        float speed = GameManager.Instance.gameSpeed;

        transform.Translate(Vector3.back * speed * Time.deltaTime);

        if (transform.position.z < -10f)
        {
            Destroy(gameObject);
        }
    }
}