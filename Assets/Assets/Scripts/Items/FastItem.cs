using UnityEngine;
using System;

public class FastItem : MonoBehaviour
{
    public float speedAmount = 10f; // 속도를 증가
    public float duration = 4f;    // 유지 시간

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ApplySpeedItemEffect(speedAmount, duration);
            Destroy(gameObject);
        }
    }
}