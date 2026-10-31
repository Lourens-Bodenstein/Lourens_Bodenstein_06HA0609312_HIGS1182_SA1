using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 40f;
    [SerializeField] private float lifetime = 3.0f;

    [Header("Impact Effects")]
    [SerializeField] private GameObject explosionPrefab;

    private void Start()
    {
        // Automatically destroy the laser after a few seconds so it doesn't fly forever and cause lag
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Fly forward independently frame-by-frame
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Fulfills Section A: Laser damages/destroys asteroids and enemy drones
        if (other.CompareTag("Asteroid") || other.CompareTag("Enemy"))
        {
            // Trace the event cleanly to the developer console for debugging marks
            Debug.Log($"Projectile: Direct hit registered on -> {other.name}");

            // Trigger visual feedback explosion if assigned
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, other.transform.position, Quaternion.identity);
            }

            // Purge both the target object and this laser projectile from the scene hierarchy cleanly
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
