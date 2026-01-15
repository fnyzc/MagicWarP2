using UnityEngine;

public class PlayerController_sc : MonoBehaviour
{
    private Health_sc hp;

    [Header("Movement")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    
    [Header("Magic Bolt")]
    public GameObject magicBoltPrefab;
    public float boltSpeed = 10f;
    public float boltDamage = 20f;

    [Header("Shield")]
    public GameObject shieldPrefab;
    public float shieldDuration = 4f;
    private bool shieldActive = false;
    public float shieldCooldown = 9f;
    private float nextShieldTime= 0f;

    [Header("Blink")]
    public float minBlinkDistance = 2f;
    public float maxBlinkDistance = 5f;
    public float blinkCooldown = 5f;
    private float nextBlinkTime =0f;

    [Header("Trap Rune")]
    public GameObject trapPrefab;

    [Header("Sound Effects")]
    public AudioClip sfxMagicBolt;
    public AudioClip sfxShield;
    public AudioClip sfxBlink;
    public AudioClip sfxTrap;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hp = GetComponent<Health_sc>();

    }

    private void Update()
    {
        HandleMovementInput();
        HandleActionsInput();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    void HandleMovementInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;
    }

    void HandleActionsInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CastMagicBolt();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ActivateShield();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            BlinkRandom();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlaceTrap();
        }
    }

    void CastMagicBolt()
    {
        if (magicBoltPrefab == null) return;

        Vector2 direction = Vector2.right;

        GameObject bolt = Instantiate(magicBoltPrefab, transform.position, Quaternion.identity);
        Rigidbody2D boltRb = bolt.GetComponent<Rigidbody2D>();
        if (boltRb != null)
        {
            boltRb.linearVelocity = direction * boltSpeed;
        }

        PlayerProjectile_sc proj = bolt.GetComponent<PlayerProjectile_sc>();
        if (proj != null)
        {
            proj.damage = boltDamage;
        }
        if (AudioController_sc.Instance != null && sfxMagicBolt != null)
        AudioController_sc.Instance.PlaySfx(sfxMagicBolt);
    }

    void ActivateShield()
    {
        if (shieldActive) return;
        if(Time.time < nextShieldTime) return;
        nextShieldTime = Time.time + shieldCooldown;
        if (AudioController_sc.Instance != null && sfxShield != null)
        AudioController_sc.Instance.PlaySfx(sfxShield);
        StartCoroutine(ShieldCoroutine());
    }


    System.Collections.IEnumerator ShieldCoroutine()
{
    shieldActive = true;
    if (hp != null) hp.shieldActive = true;

    GameObject shield = Instantiate(shieldPrefab, transform);
    shield.transform.localPosition = Vector3.zero;

    yield return new WaitForSeconds(shieldDuration);

    Destroy(shield);
    shieldActive = false;
    if (hp != null) hp.shieldActive = false;
}


    void BlinkRandom()
    {
        if(Time.time < nextBlinkTime) return;

        float angle = Random.Range(0f, Mathf.PI *2f);
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

        float distance = Random.Range(minBlinkDistance, maxBlinkDistance);
        Vector3 targetPos = transform.position + (Vector3)dir * distance;
        transform.position=targetPos;
        nextBlinkTime = Time.time + blinkCooldown;
        if (AudioController_sc.Instance != null && sfxBlink != null)
        AudioController_sc.Instance.PlaySfx(sfxBlink);
    }

    void PlaceTrap()
    {
        if (trapPrefab == null) return;
        if(PlayerTrap_sc.TrapExists) return;
        Instantiate(trapPrefab, transform.position, Quaternion.identity);
        if (AudioController_sc.Instance != null && sfxTrap != null)
        AudioController_sc.Instance.PlaySfx(sfxTrap);
    }
}
