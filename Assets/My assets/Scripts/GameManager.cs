using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject restartButton;

    public static GameManager Instance; // Singleton for easy access

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ClientRpc]
    public void GameOverClientRpc()
    {
        Debug.Log("Game Over triggered. Displaying Game Over UI.");

        // Show game over UI
        gameOverScreen.SetActive(true);

        // Disable all player movement
        DisablePlayerMovement();

        // Show the restart button
        if (NetworkManager.Singleton.IsServer)
        {
            restartButton.SetActive(true);
        }
    }

    private void DisablePlayerMovement()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var movementScript = player.GetComponent<PlayerMovement>();
            if (movementScript != null)
            {
                movementScript.enabled = false;
                movementScript.ResetAnimation();
            }
        }
    }

    public void OnRestartButtonClicked()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            RestartLevel(); // Host restarts directly
        }
        else
        {
            RestartGameServerRpc(); // Client sends a request to the server
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RestartGameServerRpc(ServerRpcParams rpcParams = default)
    {
        RestartLevel(); // Trigger the restart on the server
    }

    private void RestartLevel()
    {
        // Prevent execution on non-server instances
        if (!IsServer)
        {
            Debug.LogError("RestartLevel was called on a non-server instance. This should not happen.");
            return;
        }

        // Reset the collectible counter
        Collectible.ResetCollectibles();

        // Despawn all existing players
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var networkObject = player.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Despawn(true); // Despawn and remove from clients
            }
        }

        // Reload the current scene for all clients
        NetworkManager.Singleton.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);

        // Ensure UI elements are reset
        gameOverScreen.SetActive(false);
        restartButton.SetActive(false);

        // Respawn players once the scene is reloaded
        NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneLoadedForRespawn;
    }

    private void OnSceneLoadedForRespawn(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                RespawnPlayers();
            }

            // Unsubscribe to avoid multiple calls
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneLoadedForRespawn;
        }
        else
        {
            Debug.Log($"Unexpected SceneEventType: {sceneEvent.SceneEventType}");
        }
    }

    private void RespawnPlayers()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab);
            playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);
        }
    }
}
