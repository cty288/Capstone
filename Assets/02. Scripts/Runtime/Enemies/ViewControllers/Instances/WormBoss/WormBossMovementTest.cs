using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WormBossMovementTest : MonoBehaviour
{
    private Camera mainCamera; // Reference to the main camera
    public NavMeshAgent agent; // Reference to the NavMeshAgent component attached to the object

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        agent = this.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // Shoot a ray from the camera through the mouse position
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 5f);
            
            // If the ray hits something
            if (Physics.Raycast(ray, out hit))
            {
                agent.enabled = true;
                // Check if the object hit has a NavMesh surface
                NavMeshHit navMeshHit;
                if (NavMesh.SamplePosition(hit.point, out navMeshHit, 1.0f, NavMesh.AllAreas))
                {
                    // Move the object to the hit point
                    agent.SetDestination(navMeshHit.position);
                }
            }
        }
    }
}
