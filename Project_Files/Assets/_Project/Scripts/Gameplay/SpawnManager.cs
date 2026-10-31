using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Spawn Prefab Templates")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Location Configurations")]
    [SerializeField] private Vector3 playerSpawnPoint = new Vector3(0f, 0.4f, 0f);
    [SerializeField] private float enemySpawnRadius = 30f;
    [SerializeField] private float spawnHeightY = 0.4f;

    [Header("Runtime Enemy Spawning Balancer")]
    [SerializeField] private int initialEnemyCount = 3;
    [SerializeField] private float baseSpawnInterval = 5f;

    private float spawnTimer = 0f;
    private Transform activePlayerTransform;

    private void Awake()
    {
        // Enforce solid OOP Singleton properties safely
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SpawnPlayerDrone();
        SpawnInitialEnemyWave();
    }

    private void Update()
    {
        // Guard clause: Only run timers if the GameManager state is active
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        HandleDynamicEnemySpawning();
    }

    // Required single-responsibility custom method for Player runtime instantiation
    public void SpawnPlayerDrone()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("SpawnManager: Player Prefab reference missing in inspector!");
            return;
        }

        GameObject playerInstance = Instantiate(playerPrefab, playerSpawnPoint, Quaternion.identity);
        activePlayerTransform = playerInstance.transform;

        Debug.Log("SpawnManager: Player Drone successfully spawned at gameplay grid position.");
    }

    private void SpawnInitialEnemyWave()
    {
        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnSingleEnemy();
        }
    }

    private void HandleDynamicEnemySpawning()
    {
        spawnTimer += Time.deltaTime;

        // Dynamic difficulty scaling adjustment hook: divide by AsteroidSpawnModifier to spawn faster over time
        float dynamicInterval = baseSpawnInterval;
        if (DifficultyManager.Instance != null)
        {
            dynamicInterval /= DifficultyManager.Instance.AsteroidSpawnModifier;
        }

        if (spawnTimer >= dynamicInterval)
        {
            spawnTimer = 0f;
            SpawnSingleEnemy();
        }
    }

    // Required single-responsibility custom method for Enemy runtime instantiation
    public void SpawnSingleEnemy()
    {
        if (enemyPrefab == null) return;

        // Calculate a random circular perimeter coordinate relative to the player position
        Vector2 randomCircle = Random.insideUnitCircle.normalized * enemySpawnRadius;
        Vector3 spawnPosition = new Vector3(randomCircle.x, spawnHeightY, randomCircle.y);

        if (activePlayerTransform != null)
        {
            spawnPosition += new Vector3(activePlayerTransform.position.x, 0f, activePlayerTransform.position.z);
        }

        GameObject enemyInstance = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Check if difficulty tracking requires an immediate speed boost update on creation
        if (DifficultyManager.Instance != null && enemyInstance.TryGetComponent<EnemyAI>(out EnemyAI aiComponent))
        {
            aiComponent.UpdateDifficulty(DifficultyManager.Instance.EnemySpeedMultiplier);
        }

        Debug.Log("SpawnManager: New Enemy Starfighter spawned from Prefab asset.");
    }
}
