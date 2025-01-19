using UnityEngine;
using Unity.Netcode;

public class CollectibleSpawner : NetworkBehaviour
{
    public GameObject collectiblePrefab;
    [SerializeField] private Transform[] spawnPoints; // Assign in the Inspector

    public override void OnNetworkSpawn()
    {
        if (IsServer) // Only the server spawns collectibles
        {
            Debug.Log("Spawning Collectibles on Server Start");
            SpawnCollectibles();
        }
    }

    private void SpawnCollectibles()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points defined for collectibles!");
            return;
        }

        foreach (Transform spawnPoint in spawnPoints)
        {
            // Spawn a collectible at each spawn point
            GameObject collectible = Instantiate(collectiblePrefab, spawnPoint.position, Quaternion.identity);

            // Ensure the collectible has a NetworkObject and spawn it on the network
            NetworkObject networkObject = collectible.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn(); // Spawns for all clients
            }
            else
            {
                Debug.LogError("The collectible prefab is missing a NetworkObject component!");
            }
        }
    }
}
