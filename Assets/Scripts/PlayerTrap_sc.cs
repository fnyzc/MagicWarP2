 using UnityEngine;

public class PlayerTrap_sc : MonoBehaviour
{
    public static bool TrapExists = false;
    public float damage = 20f;
    public float slowDuration = 2f;

    private void OnEnable()
    {
        TrapExists= true;
    }

    private void OnDestroy()
    {
        TrapExists= false;
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
