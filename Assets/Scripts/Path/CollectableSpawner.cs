using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CollectableSpawner : MonoBehaviour
{

    [SerializeField]
    private GameObject Prefab;

    [SerializeField]
    private Transform Player;

    [SerializeField]
    private LineRenderer Path;

    [SerializeField]
    private float pathHeightOffset = 1.25f;

    [SerializeField]
    private float SpawnHeightOffset = 1.5f;

    [SerializeField]
    private float PathUpdateSpeed = 0.25f;

    
    private NavMeshTriangulation Triangulation;
    private Coroutine DrawPathCoroutine;

    private void Awake()
    {
        Triangulation = NavMesh.CalculateTriangulation();
    }

    private void Start()
    {
        SpawnNewObject();
    }

    private void SpawnNewObject()
    {
  
       
        if (DrawPathCoroutine != null)
        {
            StopCoroutine(DrawPathCoroutine);
        }

        DrawPathCoroutine = StartCoroutine(DrawPathToCollectable());
    }
    IEnumerator DrawPathToCollectable()
    {
        WaitForSeconds wait = new WaitForSeconds(PathUpdateSpeed);
        NavMeshPath path = new NavMeshPath();

        while(Prefab != null)
        {
            if (NavMesh.CalculatePath(Player.position, Prefab.transform.position, NavMesh.AllAreas, path))
            {
                Path.positionCount = path.corners.Length;

                for (int i = 0; i < path.corners.Length; i++)
                {
                    Path.SetPosition(i, path.corners[i] + Vector3.up * pathHeightOffset);
                }
            }
            else
            {
                Debug.Log("Unable to calculate");
            }

            yield return wait;
        }
    }
}
