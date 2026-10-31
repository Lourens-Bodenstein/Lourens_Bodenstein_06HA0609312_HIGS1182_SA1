using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 100f;

    [Header("Boundary Settings")]
    [SerializeField] private float xBoundary = 30f;
    [SerializeField] private float zBoundary = 30f;

    // --- OOP PROPERTIES FOR POWER-UPS ---
    public float Speed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    public float BaseSpeed { get; private set; }
    public float Shield { get; set; } = 100f;

    // Encapsulated tracking for active power-up states
    public bool IsInvulnerable { get; private set; } = false;
    // ------------------------------------

    // OOP Reference dependencies
    private Rigidbody rb;
    private WeaponSystem weapon;
    private EngineVisuals engineVisuals;
    private ProceduralIdleStabilizer idleStabilizer;

    private void Start()
    {
        BaseSpeed = moveSpeed;

        rb = GetComponent<Rigidbody>();
        weapon = GetComponent<WeaponSystem>();
        engineVisuals = GetComponent<EngineVisuals>();
        idleStabilizer = GetComponent<ProceduralIdleStabilizer>();

        if (rb == null) Debug.LogError("PlayerController: Missing Rigidbody!");
        if (weapon == null) Debug.LogWarning("PlayerController: No WeaponSystem component attached.");
        if (engineVisuals == null) Debug.LogWarning("PlayerController: No EngineVisuals component attached.");
        if (idleStabilizer == null) Debug.LogWarning("PlayerController: No ProceduralIdleStabilizer component attached.");
    }

    private void Update()
    {
        HandleInput();
        ClampPosition();
    }

    /// <summary>
    /// Safely updates the player's invulnerability state from external power-ups.
    /// </summary>
    public void SetInvulnerable(bool value)
    {
        IsInvulnerable = value;
    }

    public void HandleInput()
    {
        // This stops Rigidbody interaction during zero time scale, freeing the EventSystem for clicks.
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            if (engineVisuals != null) engineVisuals.UpdateEngineEffects(0f, 0f);
            return;
        }

        float verticalAxis = Input.GetAxis("Vertical");
        float horizontalAxis = Input.GetAxis("Horizontal");

        if (idleStabilizer != null)
        {
            idleStabilizer.NotifyPlayerMovement(verticalAxis, horizontalAxis);
        }

        bool isHumanControlling = Mathf.Abs(verticalAxis) > 0.01f || Mathf.Abs(horizontalAxis) > 0.01f;

        if (isHumanControlling)
        {
            float moveForward = verticalAxis * moveSpeed * Time.deltaTime;
            float turn = horizontalAxis * rotationSpeed * Time.deltaTime;

            transform.Translate(Vector3.forward * moveForward);
            transform.Rotate(Vector3.up * turn);

            if (engineVisuals != null)
            {
                engineVisuals.UpdateEngineEffects(verticalAxis, horizontalAxis);
            }
        }
        else
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (engineVisuals != null && idleStabilizer != null)
            {
                engineVisuals.UpdateEngineEffects(0f, 0f);
            }
        }

        if ((Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space)) && weapon != null)
        {
            weapon.FireWeapon();
        }
    }

    private void ClampPosition()
    {
        float clampedX = Mathf.Clamp(transform.position.x, -xBoundary, xBoundary);
        float clampedZ = Mathf.Clamp(transform.position.z, -zBoundary, zBoundary);
        transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsInvulnerable)
        {
            Debug.Log($"PlayerController: Ignored lethal contact with {collision.gameObject.name} due to active Shield.");
            return;
        }

        if (collision.gameObject.CompareTag("Asteroid") || collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("PlayerController: Lethal collision detected.");

            if (weapon != null)
            {
                weapon.ResetWeaponUpgrades();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("hit");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }
}
