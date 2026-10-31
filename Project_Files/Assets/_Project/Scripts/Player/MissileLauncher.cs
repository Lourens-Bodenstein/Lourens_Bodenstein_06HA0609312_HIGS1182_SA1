using UnityEngine;
using TMPro;

public class MissileLauncher : MonoBehaviour
{
    [Header("Launcher Settings")]
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private float fireCooldown = 1f;

    [Header("Ammunition Settings")]
    [SerializeField] private int maxMissiles = 5;
    private int currentMissilesAmmo;

    [Header("UI Feedback Hook (Auto-Assigned at Runtime)")]
    private TextMeshProUGUI activeMissileText;

    private float nextFireTime = 0f;

    // Cache reference to our own ship's collider to bypass overlap physics
    private Collider playerCollider;

    private void Start()
    {
        currentMissilesAmmo = maxMissiles;

        // Cache the player's collider component on start up
        playerCollider = GetComponentInParent<Collider>();
        if (playerCollider == null)
        {
            playerCollider = GetComponent<Collider>();
        }

        GameObject uiObj = GameObject.FindGameObjectWithTag("MissileUI");
        if (uiObj != null)
        {
            activeMissileText = uiObj.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogWarning("MissileLauncher: Could not find a UI element tagged 'MissileUI' in the scene.");
        }

        UpdateUIFeedback();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && Time.time >= nextFireTime)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                return;

            if (currentMissilesAmmo > 0)
            {
                LaunchHomingMissile();
                nextFireTime = Time.time + fireCooldown;
            }
            else
            {
                Debug.Log("MissileLauncher: Tactical failure. Magazine depleted!");
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("click");
                }
            }
        }
    }

    private void LaunchHomingMissile()
    {
        if (missilePrefab == null)
        {
            Debug.LogError("MissileLauncher: Missile Prefab unassigned in the inspector slot!");
            return;
        }

        Vector3 spawnPosition = launchPoint != null ? launchPoint.position : transform.position;
        Quaternion spawnRotation = launchPoint != null ? launchPoint.rotation : transform.rotation;

        GameObject spawnedObj = Instantiate(missilePrefab, spawnPosition, spawnRotation);

        // Prevent spawned projectile from transferring kinetic forces back to player ship
        Collider missileCollider = spawnedObj.GetComponent<Collider>();
        if (missileCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(missileCollider, playerCollider, true);
        }

        HomingMissile missileScript = spawnedObj.GetComponent<HomingMissile>();
        if (missileScript != null)
        {
            missileScript.Initialize(this);
            currentMissilesAmmo--;
            UpdateUIFeedback();
        }
    }

    public void NotifyMissileDestroyed()
    {
    }

    private void UpdateUIFeedback()
    {
        if (activeMissileText != null)
        {
            activeMissileText.text = $"Missiles (E): {currentMissilesAmmo}";
        }
    }
}
