using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HomingMissile : MonoBehaviour
{
    public enum ForwardAxis { Z_Forward, Y_Up, X_Right }

    [Header("Missile Physics")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float rotateSpeed = 250f;
    [SerializeField] private float lifetime = 5f;

    [Header("Asset Pivot Alignment")]
    [Tooltip("Change this if your 3D model shoots sideways, downwards, or twirls.")]
    [SerializeField] private ForwardAxis modelForwardAxis = ForwardAxis.Z_Forward;

    private Transform target;
    private Rigidbody rb;
    private MissileLauncher launcherRef;

    // Helper property to grab the true visual nose direction of the model
    private Vector3 ChosenForward
    {
        get
        {
            switch (modelForwardAxis)
            {
                case ForwardAxis.Y_Up: return transform.up;
                case ForwardAxis.X_Right: return transform.right;
                default: return transform.forward;
            }
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.angularVelocity = Vector3.zero;

        Destroy(gameObject, lifetime);
        LockOntoNearestEnemy();
    }

    public void Initialize(MissileLauncher launcher)
    {
        launcherRef = launcher;
    }

    private void LockOntoNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistanceSqr = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            Vector3 directionToEnemy = enemy.transform.position - transform.position;
            directionToEnemy.y = 0f; // Track on flat 2D plane layout

            float dSqr = directionToEnemy.sqrMagnitude;
            if (dSqr < shortestDistanceSqr)
            {
                shortestDistanceSqr = dSqr;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            target = nearestEnemy.transform;
        }
    }

    private void FixedUpdate()
    {
        // Always fly forward along the visual direction at design speed
        rb.linearVelocity = ChosenForward * speed;

        // If target dies or is missing, search for a new one immediately
        if (target == null)
        {
            LockOntoNearestEnemy();
            return;
        }

        // Calculate standard directional vector on a 2D plane
        Vector3 targetDir = target.position - transform.position;
        targetDir.y = 0f;

        // If we are close enough to overshoot, stop tracking adjustments 
        // and commit completely straight forward to punch directly through the target's bounding volume.
        if (targetDir.sqrMagnitude < 1.5f)
        {
            return;
        }

        targetDir.Normalize();

        if (targetDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);

            // Apply pivot adjustment corrections depending on asset modeling configuration
            if (modelForwardAxis == ForwardAxis.Y_Up)
            {
                targetRotation *= Quaternion.Euler(90, 0, 0);
            }
            else if (modelForwardAxis == ForwardAxis.X_Right)
            {
                targetRotation *= Quaternion.Euler(0, -90, 0);
            }

            // Smooth physical alignment without introducing erratic angular torque
            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.ExplodeEnemy(GameObject.FindGameObjectWithTag("Player"));
            }
            ExplodeMissile();
        }
        else if (collision.gameObject.CompareTag("Asteroid"))
        {
            ExplodeMissile();
        }
    }

    private void ExplodeMissile()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (launcherRef != null)
        {
            launcherRef.NotifyMissileDestroyed();
        }
    }
}
