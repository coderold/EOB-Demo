using System;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject lootPrefab;

    [Header("UI Reference")]
    [SerializeField] private PlayerHUDController hudController;

    [Header("Enemy UI")]
    [SerializeField] private WorldSpaceHealthBar enemyUI;
    private int currentHealth;

    public UnityEvent<int, int> OnHealthChanged; 
    public Action OnDie;

    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // Notify UI/HUD for later implementation
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{gameObject.name} took {damage} damage! Current health: {currentHealth}");

        if (enemyUI != null)
        {
            enemyUI.UpdateHealthUI(currentHealth, maxHealth);
        }

        if (hudController != null)
        {
            hudController.UpdateHealthUI(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        OnDie?.Invoke();
        Debug.Log($"{gameObject.name} has died.");
        // DropLoot();
        // Destroy(gameObject);
    }

    public void DropLoot()
    {
        Transform dropPoint = transform.Find("LootDropPoint");

        if (dropPoint != null)
        {
            Instantiate(lootPrefab, dropPoint.position, Quaternion.identity);
            Debug.Log($"{lootPrefab.name} has been dropped");
        }
        else
        {
            Debug.Log("No Loot Drop Point has been found.");
        }
    }
}