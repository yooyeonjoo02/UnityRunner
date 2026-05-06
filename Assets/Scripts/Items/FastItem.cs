using UnityEngine;

public class FastItem : MonoBehaviour
{
    public float speedMultiplier = 1.5f; // 이동 속도 1.5배
    public float timeScaleMultiplier = 1.5f; // 게임 시간 1.5배
    public float duration = 4f; // 4초 유지

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // 플레이어 속도 증가 함수 호출
                player.ApplyFastEffect(speedMultiplier, duration);
                
                // GameManager가 싱글톤(GameManager.Instance)이라고 가정하고 시간 가속 호출
                // 추후 싱글톤 처리 후 아래 주석을 해제하여 사용하세요.
                // GameManager.Instance.ApplyTimeScaleEffect(timeScaleMultiplier, duration);
            }
            
            Destroy(gameObject);
        }
    }
}