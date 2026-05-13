using UnityEngine;

public class SlowItem : MonoBehaviour
{
    public float slowAmount = -10f; // 속도를 감소
    public float duration = 3f; // 유지 시간

    public GameObject pickupParticlePrefab; // 파티클 프리팹 추가

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 파티클 생성
            if (pickupParticlePrefab != null)
            {
                Instantiate(
                    pickupParticlePrefab,
                    transform.position,
                    Quaternion.identity
                );
            }

            if (GameManager.Instance != null)
                GameManager.Instance.ApplySpeedItemEffect(slowAmount, duration, false);
            Destroy(gameObject);
        }
    }
}