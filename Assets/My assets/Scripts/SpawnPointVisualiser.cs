using UnityEngine;

public class SpawnPointVisualizer : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f); // Adjust size
    }
}
