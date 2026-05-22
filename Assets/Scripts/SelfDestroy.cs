using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    public float delay = 2f;

    void Start()
    {
        // This is safe because 'gameObject' refers to the active clone in the scene, not the asset file!
        Destroy(gameObject, delay);
    }
}