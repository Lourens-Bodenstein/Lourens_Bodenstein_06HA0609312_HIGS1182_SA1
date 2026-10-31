using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 30f; // Degrees per second
    [SerializeField] private Vector3 rotationAxis = new Vector3(0f, 1f, 0f); // Rotates around Y axis by default

    private void Update()
    {
        // Continuous, frame-rate independent rotation
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}
