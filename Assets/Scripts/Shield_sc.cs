using UnityEngine;

public class Shield_sc : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyProjectile"))
        {
            Destroy(other.gameObject);
        }
    }
}
