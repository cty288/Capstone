using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BerserkerNode : MonoBehaviour
{
    [SerializeField] private List<BerserkerNode> connectedNodes;

    [SerializeField] float sphereSize = 1.0f;
    [SerializeField] int division = 100;
    [SerializeField] float speed = 3.0f;
    [SerializeField] float moveSphereSize = 1.0f;
    [SerializeField] float moveOffset = 0.1f;
    [SerializeField] int sphereNum = 5;

    private void OnValidate()
    {
        foreach (BerserkerNode node in connectedNodes)
        {
            if (node == null)
            {
                connectedNodes.Remove(node);
            }
            
            if (!node.connectedNodes.Contains(this))
            {
                node.connectedNodes.Add(this);
            }
        }
    }

    // if previous node is specified, the returned node will not be the previous node
    public BerserkerNode GetRandomNode(BerserkerNode previousNode = null)
    {
        if (previousNode == null)
            return connectedNodes[Random.Range(0, connectedNodes.Count)];
        
        List<BerserkerNode> temp = new List<BerserkerNode>(connectedNodes);
        temp.Remove(previousNode);
        BerserkerNode randomNode = temp[Random.Range(0, temp.Count)];
        return randomNode;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, sphereSize);
    
        Gizmos.color = Color.red;
        foreach(BerserkerNode node in connectedNodes)
        {
            Gizmos.DrawLine(transform.position, node.transform.position);
        }
    }
}
