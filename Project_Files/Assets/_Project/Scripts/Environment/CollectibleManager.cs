using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    [SerializeField] private GameObject scrapPrefab;
    [SerializeField] private int spawnCount = 10;
    [SerializeField] private Vector3 spawnBounds = new Vector3(25f, 0f, 25f);

    private void Start()
    {
        SpawnCollectibles();
    }

    public void SpawnCollectibles()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(-spawnBounds.x, spawnBounds.x),
                0.5f, // Offset slightly high so it floats above ground planes
                Random.Range(-spawnBounds.z, spawnBounds.z)
            );

            Instantiate(scrapPrefab, spawnPos, Quaternion.identity, this.transform);
        }
        Debug.Log("Collectible Manager: Populated scrap canisters in play field.");
    }
}
