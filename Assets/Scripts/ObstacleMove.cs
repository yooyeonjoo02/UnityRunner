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

        // 고스트 모드 알파값 실시간 동기화
        if (rend != null && GameManager.Instance != null)
        {
            if (GameManager.Instance.isGhostMode)
            {
                Color c = originalColor;
                c.a = GameManager.Instance.currentGhostAlpha; // 점점 빨라지는 깜빡임 반영
                rend.material.color = c;
            }
            else
            {
                // 고스트 모드가 아닐 때는 원래 투명도 유지
                Color c = originalColor;
                c.a = 1.0f;
                rend.material.color = c;
            }
        }

        if (transform.position.z < -10f)
        {
            Destroy(gameObject);
        }
    }
}