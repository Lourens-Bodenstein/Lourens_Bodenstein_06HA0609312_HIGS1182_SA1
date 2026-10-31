using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    [Header("AI Tracking Configurations")]
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float detectionRange = 25f;

    [Header("Obstacle Avoidance Matrix")]
    [SerializeField] private LayerMask obstacleLayer; // Assign the "Asteroid" layer here
    [SerializeField] private float lookAheadDistance = 12f; // Increased distance to spot rocks sooner
    [SerializeField] private float avoidanceRadius = 3.5f; // Widened clear zone for safety
    [SerializeField] private float avoidanceForce = 20f; // Snappier steering power to prevent clipping

    [Header("Anti-Stuck Shake Preventer")]
    [Tooltip("If the enemy gets closer than this to a rock, it backs up to stop vibrating.")]
    [SerializeField] private float absoluteMinimumDistance = 4.0f;
    [SerializeField] private float backupSpeed = 4f;

    [Header("Gameplay Height Anchor")]
    [Tooltip("Keep this at the exact same Y position as your player ship.")]
    [SerializeField] private float lockedHeightY = 0.4f;

    private Transform playerTransform;
    private bool isPlayerSpotted = false;

    private void Start()
    {
        // Cache the target player object at runtime via Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("EnemyAI: No GameObject with the tag 'Player' found in the scene!");
        }
    }

    private void Update()
    {
        EvaluateAIStates();
    }

    public void EvaluateAIStates()
    {
        if (playerTransform == null) return;

        Vector3 enemyFlatPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 playerFlatPos = new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);
        float distanceToPlayer = Vector3.Distance(enemyFlatPos, playerFlatPos);

        if (distanceToPlayer <= detectionRange)
        {
            if (!isPlayerSpotted)
            {
                isPlayerSpotted = true;
                Debug.Log($"Enemy Drone Spotted Player! Shifting state to Chase.");
            }
            ChasePlayer();
        }
        else
        {
            isPlayerSpotted = false;
        }
    }

    private void ChasePlayer()
    {
        // 1. Check if we are physically stuck/too close to an asteroid to kill the shaking
        Collider[] closeObstacles = Physics.OverlapSphere(transform.position, absoluteMinimumDistance, obstacleLayer);

        if (closeObstacles.Length > 0)
        {
            // Emergency Mode: Back away from the closest asteroid to clear the collision hull
            Vector3 avoidDirection = (transform.position - closeObstacles[0].transform.position);
            avoidDirection.y = 0f;
            avoidDirection.Normalize();

            // Push the drone back out of the stuck zone
            transform.Translate(avoidDirection * backupSpeed * Time.deltaTime, Space.World);

            // Turn slowly away from the hazard point
            Quaternion escapeRotation = Quaternion.LookRotation(avoidDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, escapeRotation, Time.deltaTime * 2f);

            // Clamp position height and stop normal movement this frame
            transform.position = new Vector3(transform.position.x, lockedHeightY, transform.position.z);
            return;
        }

        // 2. Normal Pathfinding: Determine base target vector toward player
        Vector3 moveDirection = (playerTransform.position - transform.position);
        moveDirection.y = 0f;
        moveDirection.Normalize();

        // 3. Probe space ahead for drifting asteroids using a thick cylinder footprint
        Ray avoidanceRay = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.SphereCast(avoidanceRay, avoidanceRadius, out hit, lookAheadDistance, obstacleLayer))
        {
            if (hit.collider.CompareTag("Asteroid"))
            {
                // Calculate a path running flush against the wall vector of the asteroid
                Vector3 avoidanceTarget = Vector3.Cross(hit.normal, Vector3.up).normalized;

                // Keep the forward intent matching our overall move direction
                if (Vector3.Dot(avoidanceTarget, moveDirection) < 0)
                {
                    avoidanceTarget = -avoidanceTarget;
                }

                // Blend tracking movement with clean slide steering
                moveDirection = Vector3.Lerp(moveDirection, avoidanceTarget, avoidanceForce * Time.deltaTime).normalized;
                moveDirection.y = 0f;
            }
        }

        // 4. Smoothly turn to face the corrected clear path
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // 5. Apply translation velocity forward
        transform.Translate(Vector3.forward * chaseSpeed * Time.deltaTime);

        // 6. Hard lock vertical position grid index
        transform.position = new Vector3(transform.position.x, lockedHeightY, transform.position.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            // OOP Rule Check: If player is invulnerable, trigger self-destruction flow
            if (player != null && player.IsInvulnerable)
            {
                ExplodeEnemy(player.gameObject);
                return;
            }

            // Normal lethal interaction loop
            Debug.Log("EnemyAI: Lethal collision with Player Drone confirmed.");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    /// <summary>
    /// Handles the visual, auditory, and structural destruction of the enemy ship.
    /// </summary>
    public void ExplodeEnemy(GameObject playerReference)
    {
        Debug.Log($"EnemyAI: {gameObject.name} vaporized via external shield interaction.");

        // Dynamically borrow the visual asset from the player's weapon context to stay DRY
        WeaponSystem weaponSystem = playerReference.GetComponent<WeaponSystem>();
        if (weaponSystem != null)
        {
            // Note: Remember to change 'explosionPrefab' to public or internal in WeaponSystem.cs!
            if (weaponSystem.explosionPrefab != null)
            {
                Instantiate(weaponSystem.explosionPrefab, transform.position, Quaternion.identity);
            }
        }

        // Trigger identical audio profile used by the standard heavy weapons
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("explosion");
        }

        Destroy(gameObject);
    }

    public void UpdateDifficulty(float speedMultiplier)
    {
        chaseSpeed *= speedMultiplier;
        Debug.Log($"EnemyAI: Speed increased to {chaseSpeed} via difficulty scaling.");
    }

    private void OnDrawGizmosSelected()
    {
        // Safe sensor ranges
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * lookAheadDistance);
        Gizmos.DrawWireSphere(transform.position + transform.forward * lookAheadDistance, avoidanceRadius);

        // Critical danger boundary
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, absoluteMinimumDistance);
    }
}
