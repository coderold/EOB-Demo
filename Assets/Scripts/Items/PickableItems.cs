using UnityEngine;

public class PickableItem : MonoBehaviour
{
    public enum ItemType { Perlas, HealthPotion, ManaPotion }
    
    [Header("Item Settings")]
    public ItemType itemType;
    public int valueAmount = 1; 

    [Header("Effects")]
    public GameObject pickupEffectPrefab; 
    public AudioClip pickupSound;        
    [Range(0f, 1f)] public float soundVolume = 0.7f;

    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;
            Collect(other.gameObject);
        }
    }

    private void Collect(GameObject player)
    {
        AwardItemToPlayer(player);

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
        }

        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

        }

        Destroy(gameObject);
    }

    private void AwardItemToPlayer(GameObject player)
    {       
        switch (itemType)
        {
            case ItemType.Perlas:
                Debug.Log($"+{valueAmount} Perlas collected!");
                if (Echoes.Game.Presentation.CurrencyUIController.Instance != null)
                {
                    Echoes.Game.Presentation.CurrencyUIController.Instance.AddPerlas(valueAmount);
                }
                else
                {
                    Debug.LogWarning("CurrencyUIController Instance not found in scene! Ensure it is attached to your Currency UI panel.");
                }
                break;
                

            case ItemType.HealthPotion:
                Debug.Log($"+{valueAmount} Health restored!");
                break;
        }
    }
}