using UnityEngine;

public class ScrapCollectible : MonoBehaviour
{
    [Header("Scrap Scoring Settings")]
    [SerializeField] private int scoreValue = 10;

    private void OnTriggerEnter(Collider other)
    {
        // Verify intersection belongs exclusively to the player entity
        if (other.CompareTag("Player"))
        {
            ExecutePickupSequence();
        }
    }

    /// <summary>
    /// Encapsulated cleanup sequence executing scores, sound effects, and memory purging.
    /// </summary>
    private void ExecutePickupSequence()
    {
        Debug.Log($"ScrapCollectible: Collected item -> {gameObject.name}");

        // Execute Score increments across global instance safely
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        // Programmatically trigger the scrap pickup sound through the AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("scrap");
        }

        // Purge item instance from world hierarchy cleanly
        Destroy(gameObject);
    }
}
