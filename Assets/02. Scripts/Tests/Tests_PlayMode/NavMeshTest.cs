using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshTest : MonoBehaviour
{
    private void Update() {
        NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 50, NavMesh.AllAreas);
        Debug.DrawLine(transform.position, hit.position, Color.red);
        
    }
}
