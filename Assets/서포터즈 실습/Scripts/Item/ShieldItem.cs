using UnityEngine;

public class ShieldItem : MonoBehaviour
{
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

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // PlayerController에 추후 구현할 AddShield 함수 호출
                player.AddShield();
            }
            
            Destroy(gameObject);
        }
    }
}