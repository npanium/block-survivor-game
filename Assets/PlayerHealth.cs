using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public UnityEvent OnDeath;
    public UnityEvent<int> OnHealthChanged;

    public void TakeDamage(int damage) { }
    public void Heal(int amount) { }
    public bool IsAlive() { return true; }
}