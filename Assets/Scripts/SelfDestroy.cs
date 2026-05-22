using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    public float delay = 2f;

    void Start()
    {
        Destroy(gameObject, delay);
    }
}