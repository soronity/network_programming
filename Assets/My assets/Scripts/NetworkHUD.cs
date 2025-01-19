using UnityEngine;
using Unity.Netcode;

public class NetworkHUD : MonoBehaviour
{
    private void OnGUI()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUI.Button(new Rect(10, 10, 100, 30), "Host"))
            {
                Collectible.ResetCollectibles(); // Reset collectibles when hosting
                NetworkManager.Singleton.StartHost();
            }

            if (GUI.Button(new Rect(10, 50, 100, 30), "Client"))
            {
                NetworkManager.Singleton.StartClient();
            }

            if (GUI.Button(new Rect(10, 90, 100, 30), "Server"))
            {
                Collectible.ResetCollectibles(); // Reset collectibles when starting the server
                NetworkManager.Singleton.StartServer();
            }
        }
        else
        {
            if (GUI.Button(new Rect(10, 10, 100, 30), "Disconnect"))
            {
                Collectible.ResetCollectibles(); // Reset collectibles on disconnect
                NetworkManager.Singleton.Shutdown();
            }
        }
    }
}
