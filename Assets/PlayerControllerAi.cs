using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public class PlayerControllerAi : MonoBehaviour
{
    private NavMeshAgent myNavMeshAgent;
    // Start is called before the first frame update
    void Start()
    {
        myNavMeshAgent = GetComponent<NavMeshAgent>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ClickToMove();
        }
    }

    void ClickToMove()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool hasHit = Physics.Raycast(ray, out hit);
        if (hasHit)
        {
            SetDestination(hit.point);
        }
    }

    void SetDestination(Vector3 target)
    {
        myNavMeshAgent.SetDestination(target);
    }
}
