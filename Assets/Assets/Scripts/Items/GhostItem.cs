using UnityEngine;

public class GhostItem : MonoBehaviour
{
    public float duration = 5f; // 지속 시간 5초

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // PlayerController에 추후 구현할 ActivateGhostMode 함수 호출
                player.ActivateGhostMode(duration);
            }
            
            Destroy(gameObject);
        }
    }
}