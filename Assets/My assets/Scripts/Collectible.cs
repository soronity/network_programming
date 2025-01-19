using UnityEngine;
using Unity.Netcode;

public class Collectible : NetworkBehaviour
{
    private static int remainingCollectibles = 0; // Static counter for all collectibles
    private static Goal goalPlatform;            // Reference to the goal object

    private void Start()
    {
        if (IsServer)
        {
            remainingCollectibles++;
            if (goalPlatform == null)
            {
                goalPlatform = Object.FindFirstObjectByType<Goal>();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && IsServer)
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        remainingCollectibles--;
        if (remainingCollectibles <= 0 && goalPlatform != null)
        {
            goalPlatform.ActivateGoalServerRpc();
        }

        NetworkObject.Despawn(); // Despawn collectible for all clients
    }

    [ServerRpc(RequireOwnership = false)]
    public void CollectItemServerRpc()
    {
        CollectItem(); // Call the existing logic
    }

    // Reset the collectible counter
    public static void ResetCollectibles()
    {
        remainingCollectibles = 0;
    }
}
