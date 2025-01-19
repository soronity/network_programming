using UnityEngine;
using Unity.Netcode;

public class NetworkPrefabRegister : MonoBehaviour
{
    [SerializeField] private GameObject collectiblePrefab;

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.AddNetworkPrefab(collectiblePrefab);
            Debug.Log("Collectible prefab registered with NetworkManager.");
        }
        else
        {
            Debug.LogError("NetworkManager is not found in the scene!");
        }
    }
}
