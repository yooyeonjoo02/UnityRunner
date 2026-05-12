using UnityEngine;

public class GroundLoop : MonoBehaviour
{
    public Transform[] grounds;

    // public float moveSpeed = 10f;
    public float groundLength = 100f;
    public float resetZ = -100f;

    void Update()
    {
        float speed = GameManager.Instance.gameSpeed;

        foreach (Transform ground in grounds)
        {
            ground.Translate(Vector3.back * speed * Time.deltaTime);

            if (ground.position.z <= resetZ)
            {
                ground.position += new Vector3(0, 0, groundLength * grounds.Length);
            }
        }
    }
}