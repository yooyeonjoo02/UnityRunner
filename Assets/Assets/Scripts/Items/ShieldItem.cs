using UnityEngine;

public class ShieldItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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