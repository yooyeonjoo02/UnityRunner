using UnityEngine;

public class SlowItem : MonoBehaviour
{
    public float slowAmount = -10f; // 속도를 감소
    public float duration = 3f; // 유지 시간

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ApplySpeedItemEffect(slowAmount, duration);
            Destroy(gameObject);
        }
    }
}