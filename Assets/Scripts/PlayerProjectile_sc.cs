using UnityEngine;

public class PlayerProjectile_sc : MonoBehaviour
{
    public float damage = 15f;
    public float lifeTime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
       if (other.CompareTag("Enemy"))
        {
            Health_sc enemyHealth = other.GetComponent<Health_sc>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}
