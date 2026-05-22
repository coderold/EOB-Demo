using UnityEngine;

public class RotateLoot : MonoBehaviour
{
    public float rotationSpeed = 50f;
    public float floatFrequency = 0.5f;
    public float floatAmplitude = 2f;

    private Vector3 startPos;

    void Start() => startPos = transform.position;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        Vector3 tempPos = startPos;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * floatFrequency) * floatAmplitude;
        transform.position = tempPos;
    }
}