using UnityEngine;

public class PickableItem : MonoBehaviour
{
    public enum ItemType { Perlas, HealthPotion, ManaPotion }
    
    [Header("Item Settings")]
    public ItemType itemType;
    public int valueAmount = 1; // How many/much this pickup gives

    [Header("Effects")]
    public GameObject pickupEffectPrefab; // Optional particle system prefab
    public AudioClip pickupSound;         // Optional pickup sound effect
    [Range(0f, 1f)] public float soundVolume = 0.7f;

    private bool isCollected = false;

    // Unity automatically calls this when another collider enters the Trigger zone
    private void OnTriggerEnter(Collider other)
    {
        // 1. Prevent duplicate collection triggers if the player hits it rapidly
        if (isCollected) return;

        // 2. Check if the object touching us is the Player (Make sure your Player has the "Player" Tag!)
        if (other.CompareTag("Player"))
        {
            isCollected = true;
            Collect(other.gameObject);
        }
    }

    private void Collect(GameObject player)
    {
        // 3. Award the item to the player system
        AwardItemToPlayer(player);

        // 4. Play audio clip if one is assigned
        if (pickupSound != null)
        {
            // Plays audio in world space at this position so it doesn't get cut off when the object is destroyed
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
        }

        // 5. Spawn a burst particle effect if assigned
        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);

        }

        // 6. Delete the item asset from the world
        Destroy(gameObject);
    }

    private void AwardItemToPlayer(GameObject player)
    {       
        switch (itemType)
        {
            case ItemType.Perlas:
                Debug.Log($"+{valueAmount} Perlas collected!");
                // Example hook: player.GetComponent<PlayerInventory>().AddPerlas(valueAmount);
                break;

            case ItemType.HealthPotion:
                Debug.Log($"+{valueAmount} Health restored!");
                // Example hook: player.GetComponent<Health>().Heal(valueAmount);
                break;
        }
    }
}