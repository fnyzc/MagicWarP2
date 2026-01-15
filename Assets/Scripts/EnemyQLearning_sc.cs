using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class EnemyQLearning_sc : MonoBehaviour
{
    public static EnemyQLearning_sc Instance { get; private set; }

    private QLearningBrain brain;

    [Header("References")]
    public Transform player;
    private EnemyController_sc enemy;
    private Health_sc enemyHealth;

    [Header("Decision Timing")]
    [Tooltip("How often the agent decides an action. Prevents 'spamming' every frame.")]
    public float decisionInterval = 0.30f;

    private float decisionTimer = 0f;
    private float lastDistanceToPlayer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        brain = GetComponent<QLearningBrain>();
        enemy = GetComponent<EnemyController_sc>();
        enemyHealth = GetComponent<Health_sc>();
    }

    private void Start()
    {
        Debug.Log($"[AI MODE] Loaded={brain.IsModelLoaded} | Random={brain.forceRandomActions}");

        if (brain == null || enemy == null)
        {
            Debug.LogError("EnemyQLearning_sc: QLearningBrain veya EnemyController_sc yok!");
            enabled = false;
            return;
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
            lastDistanceToPlayer = Vector2.Distance(transform.position, player.position);

        RegisterActions();

        bool hasLoadedAi = (AiRuntimeState_sc.Instance != null && AiRuntimeState_sc.Instance.HasAi);

        if (hasLoadedAi)
        {
            if (brain.TryLoadFromJson(AiRuntimeState_sc.Instance.loadedAiJson, out string err))
            {

                brain.forceRandomActions = false; 
                brain.learningEnabled = false;    
                Debug.Log("EnemyQLearning_sc: AI loaded -> Trained behavior");
            }
            else
            {
                Debug.LogWarning("EnemyQLearning_sc: AI yüklenemedi, Random moda dönülüyor. Hata: " + err);
                brain.ResetToUntrainedRandom();
            }
        }
        else
        {
            brain.ResetToUntrainedRandom();
            if (brain.learningEnabled)
            Debug.Log("EnemyQLearning_sc: TRAINING MODE (AI file not required)");
            else
            Debug.Log("EnemyQLearning_sc: AI not loaded -> Random behavior");

        }

        if (GameManager_sc.Instance != null)
        {
            if (GameManager_sc.Instance.enemyHealth != null)
                GameManager_sc.Instance.enemyHealth.OnDeath += HandleEnemyDeath;

            if (GameManager_sc.Instance.playerHealth != null)
                GameManager_sc.Instance.playerHealth.OnDeath += HandlePlayerDeath;
        }
    }

    private void RegisterActions()
    {

        brain.RegisterAction("DoNothing", _ => DoNothing(), 0);

        brain.RegisterAction("Fireball", _ => enemy.CastFireball(), 0);
        brain.RegisterAction("FireRing", _ => enemy.CastFireRing(), 0);
        brain.RegisterAction("SlowCurse", _ => enemy.CastSlowCurse(), 0);

        brain.RegisterAction("HealSelf", _ => enemy.CastSelfHeal(), 0);

        brain.RegisterAction("BlinkRandom", _ => BlinkWithReward(), 0);

        brain.RegisterAction("MoveTowardPlayer", _ => MoveTowardPlayer(), 0);
        brain.RegisterAction("MoveAwayFromPlayer", _ => MoveAwayFromPlayer(), 0);
    }

    private void Update()
    {
        if (brain == null) return;

        decisionTimer += Time.deltaTime;
        if (decisionTimer < decisionInterval) return;
        decisionTimer = 0f;


        List<float> inputsBefore = BuildInputsSafe();
        brain.SetInputs(inputsBefore);

        int actionIndex;

        if (brain.learningEnabled)
        {

        actionIndex = brain.DecideAction();
        }
        else if (brain.forceRandomActions)
        {
    
        actionIndex = Random.Range(0, Mathf.Max(1, brain.ActionCount));
        }
        else
        {
   
        actionIndex = brain.DecideAction();
        }


        brain.ExecuteAction(actionIndex);

        List<float> inputsAfter = BuildInputsSafe();
        brain.SetInputs(inputsAfter);

        if (brain.learningEnabled)
            HandleSimpleRewards();
    }

    private List<float> BuildInputsSafe()
    {
        var inputs = new List<float>();

        float dist = 999f;
        if (player != null)
            dist = Vector2.Distance(transform.position, player.position);
        inputs.Add(dist);

        float hp = enemyHealth != null ? enemyHealth.currentHealth : 100f;
        inputs.Add(hp);

        float playerHp = 100f;
        if (GameManager_sc.Instance != null && GameManager_sc.Instance.playerHealth != null)
            playerHp = GameManager_sc.Instance.playerHealth.currentHealth;
        inputs.Add(playerHp);

        return inputs;
    }

    private void HandleSimpleRewards()
    {
        if (player == null) return;

        float currentDist = Vector2.Distance(transform.position, player.position);

        if (currentDist < lastDistanceToPlayer) brain.Reward(1f);
        else if (currentDist > lastDistanceToPlayer) brain.Punish(0.5f);

        lastDistanceToPlayer = currentDist;
    }

    private void DoNothing() { }

    private void BlinkWithReward()
    {
        if (enemy == null) return;

        if (player == null)
        {
            enemy.BlinkRandom();
            return;
        }

        float beforeDist = Vector2.Distance(enemy.transform.position, player.position);
        enemy.BlinkRandom();
        float afterDist = Vector2.Distance(enemy.transform.position, player.position);

        if (!brain.learningEnabled) return;

        if (afterDist + 0.5f < beforeDist) brain.Reward(2f);
        else brain.Punish(10f);
    }

    private void MoveTowardPlayer()
    {
        if (enemy == null || player == null) return;
        Vector2 dir = (Vector2)(player.position - enemy.transform.position);
        enemy.MoveStep(dir);
    }

    private void MoveAwayFromPlayer()
    {
        if (enemy == null || player == null) return;
        Vector2 dir = (Vector2)(enemy.transform.position - player.position);
        enemy.MoveStep(dir);
    }

    private void HandleEnemyDeath()
{
    if (brain != null && brain.learningEnabled)
        brain.Punish(50f); 

}

private void HandlePlayerDeath()
{
    if (brain != null && brain.learningEnabled)
        brain.Reward(30f); 

}


    public void OnDamagePlayer(float damage)
    {
        if (brain == null) return;
        if (!brain.learningEnabled) return;

        brain.Reward(Mathf.Clamp(damage, 0f, 50f) * 1.0f);
    }

    public void OnTakeDamage(float damage)
    {
        if (brain == null) return;
        if (!brain.learningEnabled) return;

        brain.Punish(Mathf.Clamp(damage, 0f, 50f) * 1.0f);
    }

    public void OnGoodHeal() => OnGoodHeal(10f);
    public void OnWastedHeal() => OnWastedHeal(10f);

    public void OnGoodHeal(float healAmount)
    {
        if (brain == null) return;
        if (!brain.learningEnabled) return;

        brain.Reward(Mathf.Clamp(healAmount, 0f, 50f) * 0.5f);
    }

    public void OnWastedHeal(float healAmount)
    {
        if (brain == null) return;
        if (!brain.learningEnabled) return;

        brain.Punish(Mathf.Clamp(healAmount, 0f, 50f) * 0.5f);
    }


}
