using UnityEngine;
using System.Collections;

public class WeaponSystem : MonoBehaviour
{
    [System.Serializable]
    public struct FirePointConfig
    {
        public string pointName;
        public Transform transformRef;
        [HideInInspector] public LineRenderer cachedLineRenderer;
    }

    [Header("Weapon Stats (Upgradable)")]
    [SerializeField] private float fireRange = 50f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private LayerMask collisionLayers;

    [Header("Weapon Upgrade Progression")]
    [Tooltip("0 = 1 Center Gun, 1 = 2 Inner Guns, 2 = 3 Guns (Center + Inners), 3 = 5 Guns (All On)")]
    [Range(0, 3)][SerializeField] private int currentUpgradeLevel = 0;

    [Header("Fire Point Array Definition")]
    [Tooltip("Element 0: Center\nElement 1: Left Inner\nElement 2: Right Inner\nElement 3: Left Tip\nElement 4: Right Tip")]
    [SerializeField] private FirePointConfig[] weaponFirePoints = new FirePointConfig[5];

    [Header("Visual Effects")]
    [SerializeField] public GameObject explosionPrefab;
    [SerializeField] private float laserDuration = 0.1f;

    private float nextFireTime = 0f;

    private void Start()
    {
        InitializeFirePointComponents();
    }

    private void InitializeFirePointComponents()
    {
        for (int i = 0; i < weaponFirePoints.Length; i++)
        {
            if (weaponFirePoints[i].transformRef != null)
            {
                weaponFirePoints[i].cachedLineRenderer = weaponFirePoints[i].transformRef.GetComponent<LineRenderer>();

                if (weaponFirePoints[i].cachedLineRenderer == null)
                {
                    Debug.LogWarning($"WeaponSystem: Component missing a LineRenderer on point: '{weaponFirePoints[i].pointName}'");
                }
                else
                {
                    weaponFirePoints[i].cachedLineRenderer.enabled = false;
                }
            }
        }
    }

    public void FireWeapon()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireRate;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("laser");
        }

        DetermineActiveGunsAndFire();
    }

    private void DetermineActiveGunsAndFire()
    {
        switch (currentUpgradeLevel)
        {
            case 0:
                ExecuteRaycast(weaponFirePoints[0]);
                break;

            case 1:
                ExecuteRaycast(weaponFirePoints[1]);
                ExecuteRaycast(weaponFirePoints[2]);
                break;

            case 2:
                ExecuteRaycast(weaponFirePoints[0]);
                ExecuteRaycast(weaponFirePoints[1]);
                ExecuteRaycast(weaponFirePoints[2]);
                break;

            case 3:
                for (int i = 0; i < weaponFirePoints.Length; i++)
                {
                    ExecuteRaycast(weaponFirePoints[i]);
                }
                break;
        }
    }

    private void ExecuteRaycast(FirePointConfig fireConfig)
    {
        if (fireConfig.transformRef == null) return;

        Vector3 origin = fireConfig.transformRef.position;
        Vector3 direction = fireConfig.transformRef.forward;
        Ray ray = new Ray(origin, direction);
        RaycastHit hit;
        Vector3 targetPoint = origin + (direction * fireRange);

        if (Physics.Raycast(ray, out hit, fireRange, collisionLayers))
        {
            targetPoint = hit.point;

            if (hit.collider.CompareTag("Asteroid") || hit.collider.CompareTag("Enemy"))
            {
                Debug.Log($"WeaponSystem: Target destruction verified on object -> {hit.collider.name}");

                if (explosionPrefab != null)
                {
                    Instantiate(explosionPrefab, hit.transform.position, Quaternion.identity);
                }
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("explosion");
                }
                Destroy(hit.collider.gameObject);
            }
        }

        if (fireConfig.cachedLineRenderer != null)
        {
            StartCoroutine(RenderLaserBeam(fireConfig.cachedLineRenderer, origin, targetPoint));
        }
    }

    private IEnumerator RenderLaserBeam(LineRenderer line, Vector3 start, Vector3 end)
    {
        line.enabled = true;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        yield return new WaitForSeconds(laserDuration);

        line.enabled = false;
    }

    public void TriggerWeaponUpgrade()
    {
        if (currentUpgradeLevel < 3)
        {
            currentUpgradeLevel++;
            Debug.Log($"WeaponSystem: Weapon level upgraded to tier: {currentUpgradeLevel}");
        }
    }

    public void ResetWeaponUpgrades()
    {
        currentUpgradeLevel = 0;
        Debug.Log("WeaponSystem: Weapon level reset to baseline tier.");
    }

    public void UpgradeRange(float extraRange)
    {
        fireRange += extraRange;
        Debug.Log($"WeaponSystem: Range boosted to: {fireRange}");
    }

    public void UpgradeFireRate(float speedMultiplier)
    {
        fireRate *= speedMultiplier;
        Debug.Log($"WeaponSystem: Firing cooldown set to: {fireRate}");
    }
}
