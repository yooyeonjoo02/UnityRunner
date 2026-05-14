using UnityEngine;

public class ObstacleMove : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        // 머티리얼 렌더러 캐싱 (반투명을 위해 Rendering Mode가 Transparent나 Fade여야 함)
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    void Update()
    {
        transform.Translate(Vector3.back * GameManager.Instance.gameSpeed * Time.deltaTime);

        if (transform.position.z < -10f)
        {
            Destroy(gameObject);
        }
    }
}