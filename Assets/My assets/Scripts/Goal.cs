using UnityEngine;
using Unity.Netcode;

public class Goal : NetworkBehaviour
{
    private NetworkVariable<bool> isGoalActive = new NetworkVariable<bool>(false);

    [ServerRpc(RequireOwnership = false)] // Add the ServerRpc attribute
    public void ActivateGoalServerRpc()
    {
        isGoalActive.Value = true; // Update the goal state for all clients
        Debug.Log("Goal activated!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isGoalActive.Value)
        {
            Debug.Log("Player reached the goal!");
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        if (IsServer) // Ensure the method is called on the server
        {
            if (GameManager.Instance != null) // Ensure GameManager exists
            {
                GameManager.Instance.GameOverClientRpc(); // Trigger game over through GameManager
            }
            else
            {
                Debug.LogError("GameManager instance is null! Cannot trigger game over.");
            }
        }
    }
}
