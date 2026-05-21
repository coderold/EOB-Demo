using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject lootPrefab;
    private int currentHealth;

    // Decoupled architecture: Fires an event so the HUD can listen to health changes
    public UnityEvent<int, int> OnHealthChanged; 
    public UnityEvent OnDie;

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
        
        // Notify UI/HUD
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{gameObject.name} took {damage} damage! Current health: {currentHealth}");

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
        DropLoot();
        Destroy(gameObject);
        
        // Handle loot drop logic here on Day 3
    }

    private void DropLoot()
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