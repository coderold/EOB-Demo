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
        // Safety check to prevent dividing by zero
        if (maxHealth <= 0) return;

        // Calculate fraction value between 0.0f and 1.0f
        float fillPercentage = (float)currentHealth / maxHealth;
        
        // Instantly updates the slider graphic fill
        healthBarFill.fillAmount = fillPercentage;

        // Updates the numbers cleanly on screen (e.g. "85 / 100")
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }
}