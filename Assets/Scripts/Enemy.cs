using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    [Header("Boss Stats")]
    [SerializeField] private float bossSpeed = 75f;
    [SerializeField] private int bossHealth = 180;
    [SerializeField] private int bossDamage = 18;
    [SerializeField] private int bossShield = 25;

    [Header("Current Stats")]
    private int currentHealth;
    private int currentShield;

    [Header("Events")]
    public UnityEvent OnDeath;
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent<int> OnShieldChanged;
    public UnityEvent OnShieldBroken;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color damageColor = Color.gray;
    [SerializeField] private float damageFlashDuration = 0.1f;

    private bool isDead = false;

    void Start()
    {
        // Initialize current stats
        currentHealth = bossHealth;
        currentShield = bossShield;

        // Get sprite renderer if not assigned
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Method to update boss stats from API
    public void SetStats(float speed, int health, int damage, int shield)
    {
        bossSpeed = speed;
        // bossHealth = health;
        bossDamage = damage;
        bossShield = shield;

        // Update current stats if this is called during gameplay
        currentHealth = bossHealth;
        currentShield = bossShield;

        Debug.Log($"Boss stats updated - Speed: {speed}, Health: {health}, Damage: {damage}, Shield: {shield}");
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // Shield absorbs damage first
        if (currentShield > 0)
        {
            int shieldDamage = Mathf.Min(damage, currentShield);
            currentShield -= shieldDamage;
            damage -= shieldDamage;

            OnShieldChanged?.Invoke(currentShield);

            if (currentShield <= 0)
            {
                OnShieldBroken?.Invoke();
            }
        }

        // Remaining damage goes to health
        if (damage > 0)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            OnHealthChanged?.Invoke(currentHealth);
        }

        // Visual feedback
        StartCoroutine(DamageFlash());

        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, bossHealth); // Cap at max health

        OnHealthChanged?.Invoke(currentHealth);
    }

    public void RestoreShield(int shieldAmount)
    {
        if (isDead) return;

        currentShield += shieldAmount;
        currentShield = Mathf.Min(currentShield, bossShield); // Cap at max shield

        OnShieldChanged?.Invoke(currentShield);
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        OnDeath?.Invoke();

        // Disable components
        if (GetComponent<EnemyShooting>() != null)
            GetComponent<EnemyShooting>().enabled = false;

        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = false;

        // Optional: Add death animation here

        // Destroy after delay (for death effects)
        Destroy(gameObject, 1f);
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = normalColor;
        }
    }

    // Getters for current stats
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => bossHealth;
    public int GetCurrentShield() => currentShield;
    public int GetMaxShield() => bossShield;
    public float GetSpeed() => bossSpeed;
    public int GetDamage() => bossDamage;
    public bool IsAlive() => !isDead;
    public bool HasShield() => currentShield > 0;

    // Health percentage for UI
    public float GetHealthPercentage() => (float)currentHealth / bossHealth;
    public float GetShieldPercentage() => (float)currentShield / bossShield;

    // Optional: Damage player on collision
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(bossDamage);
            }
        }
    }

    // Debug info in inspector
    void OnValidate()
    {
        // Ensure stats are positive
        bossSpeed = Mathf.Max(0, bossSpeed);
        bossHealth = Mathf.Max(1, bossHealth);
        bossDamage = Mathf.Max(0, bossDamage);
        bossShield = Mathf.Max(0, bossShield);
    }
}