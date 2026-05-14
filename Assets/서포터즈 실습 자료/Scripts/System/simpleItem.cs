using UnityEngine;

public class simpleItem : MonoBehaviour
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
            Destroy(gameObject);
        }
    }
}