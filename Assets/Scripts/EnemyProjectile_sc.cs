using UnityEngine;

public class EnemyProjectile_sc : MonoBehaviour
{
    public float damage = 15f;
    public float lifeTime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            Health_sc hp = other.GetComponent<Health_sc>();
            if (hp != null)
            {
                hp.TakeDamage(damage);

                if (EnemyQLearning_sc.Instance != null)
                    EnemyQLearning_sc.Instance.OnDamagePlayer(damage);
            }
            Destroy(gameObject);
        }
    }
}
