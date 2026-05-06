using UnityEngine;

public class SlowItem : MonoBehaviour
{
    public float slowMultiplier = 0.5f; // 속도를 절반으로
    public float duration = 3f;         // 지속 시간 3초

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // PlayerController에 추후 구현할 ApplySlowEffect 함수 호출
                player.ApplySlowEffect(slowMultiplier, duration);
            }
            
            // 아이템 파괴
            Destroy(gameObject);
        }
    }
}