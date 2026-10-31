using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [SerializeField] private GameObject shieldUpPrefab;
    [SerializeField] private GameObject speedUpPrefab;
    [SerializeField] private GameObject weaponsUpPrefab;

    [SerializeField] private float spawnInterval = 10f; // Time interval between power-up spawns
    [SerializeField] private Vector3 spawnBounds = new Vector3(25f, 0f, 25f);
    [SerializeField] private LayerMask obstacleLayer; // Layer mask for obstacles (e.g., other power-ups, asteroids)

    private List<GameObject> powerUpPrefabs;
    private float nextSpawnTime;

    private void Start()
    {
        InitializePowerUps();
        ScheduleNextSpawn();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            CheckForSpawn();
        }
    }

    private void InitializePowerUps()
    {
        powerUpPrefabs = new List<GameObject>
        {
            shieldUpPrefab,
            speedUpPrefab,
            weaponsUpPrefab
        };
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    private void CheckForSpawn()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnPowerUp();
            ScheduleNextSpawn();
        }
    }

    private void SpawnPowerUp()
    {
        int randomIndex = Random.Range(0, powerUpPrefabs.Count);
        GameObject selectedPowerUp = powerUpPrefabs[randomIndex];

        Vector3 spawnPos = GetValidSpawnPosition();

        if (spawnPos != Vector3.zero)
        {
            Instantiate(selectedPowerUp, spawnPos, Quaternion.identity);
            Debug.Log($"PowerUpManager: Spawned {selectedPowerUp.name} at position {spawnPos}");
        }
    }

    private Vector3 GetValidSpawnPosition()
    {
        int maxAttempts = 10; // Limit the number of attempts to find a valid position
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(-spawnBounds.x, spawnBounds.x),
                0.5f, // Offset slightly high so it floats above ground planes
                Random.Range(-spawnBounds.z, spawnBounds.z)
            );

            if (IsPositionValid(spawnPos))
            {
                return spawnPos;
            }
        }

        Debug.LogWarning("PowerUpManager: Unable to find a valid spawn position after max attempts.");
        return Vector3.zero; // Return zero vector if no valid position found
    }

    private bool IsPositionValid(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 1f, obstacleLayer); // Adjust radius as needed

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject && !collider.isTrigger) // Ignore self and trigger colliders
            {
                return false;
            }
        }

        return true;
    }
}
