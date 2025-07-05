using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Health Sprites (0 = 100%, 20 = 0%)")]
    [SerializeField] private Sprite[] healthSprites = new Sprite[0]; // 0-20
    [SerializeField] private SpriteRenderer healthRenderer;

    [Header("Visual Effects")]
    [SerializeField] private bool enableDamageFlash = true;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float damageFlashDuration = 0.2f;

    public UnityEvent OnDeath;
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnPlayerRevived;

    private bool isDead = false;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthRenderer == null)
            healthRenderer = GetComponent<SpriteRenderer>();

        if (healthRenderer != null)
            originalColor = healthRenderer.color;

        UpdateHealthSprite();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

        UpdateHealthSprite();
        OnHealthChanged?.Invoke(currentHealth);

        if (enableDamageFlash)
            StartCoroutine(DamageFlash());

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        int oldHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        Debug.Log($"Player healed {currentHealth - oldHealth}. Health: {currentHealth}/{maxHealth}");

        if (isDead && currentHealth > 0)
        {
            Revive();
        }

        UpdateHealthSprite();
        OnHealthChanged?.Invoke(currentHealth);
    }

    public bool IsAlive()
    {
        return !isDead && currentHealth > 0;
    }

    void UpdateHealthSprite()
    {
        if (healthRenderer == null) return;

        // Calculate sprite index (0 = 100%, 21 = 0%)
        float healthPercentage = (float)currentHealth / maxHealth;
        int maxIndex = healthSprites.Length - 1;
        int spriteIndex = Mathf.RoundToInt((1f - healthPercentage) * maxIndex);
        spriteIndex = Mathf.Clamp(spriteIndex, 0, healthSprites.Length - 1);

        if (healthSprites[spriteIndex] != null)
        {
            healthRenderer.sprite = healthSprites[spriteIndex];
        }

        Debug.Log($"Health: {currentHealth}/{maxHealth} ({healthPercentage:P0}) → Sprite {spriteIndex}");
    }

    void Die()
    {
        isDead = true;
        OnDeath?.Invoke();

        Debug.Log("Player died!");

        // Disable player controls
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null)
            shooting.enabled = false;
    }

    void Revive()
    {
        isDead = false;
        OnPlayerRevived?.Invoke();

        Debug.Log("Player revived!");

        // Re-enable player controls
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = true;

        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null)
            shooting.enabled = true;
    }

    System.Collections.IEnumerator DamageFlash()
    {
        if (healthRenderer == null) yield break;

        healthRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(damageFlashDuration);
        healthRenderer.color = originalColor;
    }

    // Additional utility methods
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;
    public bool IsCriticalHealth() => GetHealthPercentage() <= 0.25f;
    public void FullHeal() => Heal(maxHealth);
}