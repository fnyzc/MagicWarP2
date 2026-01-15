using UnityEngine;

public class Health_sc : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public System.Action OnDeath;

    public bool shieldActive;

    private void Awake()
    {
        currentHealth = maxHealth;
    }
 
    public void TakeDamage(float amount)
    {
        if (shieldActive)
        return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (CompareTag("Enemy"))
        {
        if (EnemyQLearning_sc.Instance != null)
            EnemyQLearning_sc.Instance.OnTakeDamage(amount);
        }
        
        if (CompareTag("Player"))
        {
        if (EnemyQLearning_sc.Instance != null)
            EnemyQLearning_sc.Instance.OnDamagePlayer(amount);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"{gameObject.name} healed {amount}. HP: {currentHealth}");
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");

        OnDeath?.Invoke();

        Destroy(gameObject);
    }
}
