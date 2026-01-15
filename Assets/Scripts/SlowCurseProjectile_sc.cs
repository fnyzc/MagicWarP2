using System.Collections;
using UnityEngine;

public class SlowCurseProjectile_sc : MonoBehaviour
{
    [Header("Curse Settings")]
    public float damage = 5f;
    public float slowDuration = 2f;
    public float slowMultiplier = 0.5f; 

    public float lifeTime = 5f;

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
            PlayerController_sc player = other.GetComponent<PlayerController_sc>();
            if (player != null)
            {
                StartCoroutine(ApplySlow(player));
            }
            Destroy(gameObject);
        }
    }

    private IEnumerator ApplySlow(PlayerController_sc player)
    {
        float originalSpeed = player.moveSpeed;
        player.moveSpeed = originalSpeed * slowMultiplier;

        Debug.Log("Player slowed");

        yield return new WaitForSeconds(slowDuration);

        if (player != null)
        {
            player.moveSpeed = originalSpeed;
            Debug.Log("Player slow ended.");
        }
    }
}
