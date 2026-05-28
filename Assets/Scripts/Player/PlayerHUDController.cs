using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUDController : MonoBehaviour
{
    [Header("UI Object References")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TextMeshProUGUI healthText;

    /// <summary>
    /// Call this function whenever the player takes damage or gets healed.
    /// </summary>
    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (maxHealth <= 0) return;

        float fillPercentage = (float)currentHealth / maxHealth;
        
        healthBarFill.fillAmount = fillPercentage;

        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }
}