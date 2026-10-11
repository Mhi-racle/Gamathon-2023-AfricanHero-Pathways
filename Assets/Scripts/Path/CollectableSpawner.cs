using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Draws a guide line from Ananse towards the gourd, refreshed a few times per second.
/// </summary>
public class CollectableSpawner : MonoBehaviour
{
    private const int MaxCorners = 64;

    [Tooltip("The gourd the line points to.")]
    [SerializeField]
    private GameObject Prefab;

    [SerializeField]
    private Transform Player;

    [SerializeField]
    private LineRenderer Path;

    [SerializeField]
    private float pathHeightOffset = 1.25f;

    [SerializeField]
    private float PathUpdateSpeed = 0.25f;

    // Reused every refresh so drawing the line doesn't allocate.
    private readonly Vector3[] corners = new Vector3[MaxCorners];

    private void Start()
    {
        if (Prefab != null && Player != null && Path != null) StartCoroutine(DrawPathToCollectable());
    }

    private IEnumerator DrawPathToCollectable()
    {
        WaitForSeconds wait = new WaitForSeconds(PathUpdateSpeed);
        NavMeshPath path = new NavMeshPath();

        // The gourd is destroyed when collected, which ends the loop.
        while (Prefab != null)
        {
            if (NavMesh.CalculatePath(Player.position, Prefab.transform.position, NavMesh.AllAreas, path))
            {
                int count = path.GetCornersNonAlloc(corners);
                Path.positionCount = count;
                for (int i = 0; i < count; i++)
                {
                    Path.SetPosition(i, corners[i] + Vector3.up * pathHeightOffset);
                }
            }

            yield return wait;
        }

        Path.positionCount = 0;
    }
}
