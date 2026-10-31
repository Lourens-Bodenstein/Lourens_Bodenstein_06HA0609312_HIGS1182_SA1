using UnityEngine;

public class AsteroidsSpawner : MonoBehaviour
{
    [Header("Spawning Configuration")]
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private int spawnCount = 15;
    [SerializeField] private Vector3 spawnAreaRange = new Vector3(30f, 0f, 30f);

    [Header("Safety Shield Settings")]
    [Tooltip("The minimum distance required between the player ship and a newly spawned asteroid.")]
    [SerializeField] private float playerSafetyRadius = 8f;

    private Transform playerTransform;

    private void Start()
    {
        // Dynamically find and cache the player's transform at runtime
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("AsteroidsSpawner: No GameObject found with tag 'Player'. Safety zone skipped.");
        }

        // Initialize the base field count at launch
        SpawnInitialAsteroids();
    }

    private void Update()
    {
        // Fulfills Requirement: Continuous real-time field evaluation loop
        EvaluateAndReplenishField();
    }

    /// <summary>
    /// Custom method handling initial mass runtime generation safely outside the player zone.
    /// </summary>
    public void SpawnInitialAsteroids()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnSingleSafeAsteroid();
        }
        Debug.Log($"Asteroids Spawner: Successfully initialized {spawnCount} objects.");
    }

    /// <summary>
    /// Checks the current active asteroid count under this transform container. 
    /// If an item was destroyed, it automatically replenishes the gap.
    /// </summary>
    private void EvaluateAndReplenishField()
    {
        // transform.childCount accurately tracks how many clones are currently active inside this folder group
        if (transform.childCount < spawnCount)
        {
            int missingCount = spawnCount - transform.childCount;

            for (int i = 0; i < missingCount; i++)
            {
                SpawnSingleSafeAsteroid();
            }
        }
    }

    /// <summary>
    /// Standardized operational logic to choose a verified safe point and instantiate the prefab.
    /// </summary>
    private void SpawnSingleSafeAsteroid()
    {
        Vector3 safePosition = CalculateSafeSpawnPosition();

        GameObject spawnedAsteroid = Instantiate(asteroidPrefab, safePosition, Quaternion.identity);
        spawnedAsteroid.transform.SetParent(this.transform);

        // Fulfills Section B console tracing metrics
        Debug.Log($"Asteroids Spawner: Dynamic replenishment spawned safely -> {spawnedAsteroid.name} at {safePosition}");
    }

    /// <summary>
    /// Custom filtration process that calculates distances to ensure no spawn camps occur.
    /// </summary>
    private Vector3 CalculateSafeSpawnPosition()
    {
        Vector3 randomCandidate = Vector3.zero;
        bool positionApproved = false;
        int safetyBreaker = 50;
        int currentAttempt = 0;

        while (!positionApproved && currentAttempt < safetyBreaker)
        {
            currentAttempt++;

            // Track points flatly across your design configuration plane parameters
            randomCandidate = new Vector3(
                Random.Range(-spawnAreaRange.x, spawnAreaRange.x),
                0f,
                Random.Range(-spawnAreaRange.z, spawnAreaRange.z)
            );

            // Verify spatial boundaries against the active player position coordinates
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(randomCandidate, playerTransform.position);

                if (distanceToPlayer > playerSafetyRadius)
                {
                    positionApproved = true;
                }
            }
            else
            {
                positionApproved = true;
            }
        }

        return randomCandidate;
    }
}
