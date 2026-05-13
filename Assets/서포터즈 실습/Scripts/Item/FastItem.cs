using UnityEngine;
using System;

public class FastItem : MonoBehaviour
{
    public float speedAmount = 10f; // 속도를 증가
    public float duration = 4f;    // 유지 시간

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
                GameManager.Instance.ApplySpeedItemEffect(speedAmount, duration, true);
            Destroy(gameObject);
        }
    }
}