using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SplineMovementTesting : MonoBehaviour
{ 
    [SerializeField] private Transform[] nodes;
    private Transform node1;
    private Transform node2;
    private int currentIndex = 0;
    private float time = 0f;
    
    private void Start()
    {
        node1 = nodes[currentIndex];
        node2 = nodes[currentIndex + 1];
        transform.position = node1.position;
    }

    private void Update()
    {
        if (Vector3.Distance(node2.position, transform.position) <= 0.1f)
        {
            currentIndex++;
            if (currentIndex >= nodes.Length - 1)
            {
                currentIndex = 0;
            }
            node1 = nodes[currentIndex];
            node2 = nodes[currentIndex + 1];
            time = 0f;
        }

        time += Time.deltaTime;
        transform.position = CreateHermiteSplineFunction(node1.position, node2.position)(time);
        print($"Boss 4 Test: {transform.position}, node1: {node1.name}, node2: {node2.name}");
    }

    public Func<float, Vector3> CreateHermiteSplineFunction(Vector3 node1, Vector3 node2)
    {
        return t =>
        {
            float t2 = t * t;
            float t3 = t2 * t;

            float h00 = 2 * t3 - 3 * t2 + 1; // calculate basis function 1
            float h10 = t3 - 2 * t2 + t;     // calculate basis function 2
            float h01 = -2 * t3 + 3 * t2;    // calculate basis function 3
            float h11 = t3 - t2;             // calculate basis function 4

            // calculate the interpolated spline point
            Vector3 p = h00 * node1 + h10 * (node2 - node1) + h01 * node2 + h11 * (node2 - node1);

            return p;
        };
    }
}
