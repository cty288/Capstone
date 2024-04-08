using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using  Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.Enemies.ViewControllers.Instances.Berserker;
using UnityEngine;

namespace _02._Scripts.Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class BerserkerFlyMove: EnemyAction<BerserkerEntity>
    {
        private TaskStatus taskStatus;

        public SharedGameObject finalPosition;
        private BerserkerNode currentNode;
        
        private List<BerserkerNode> movementNodes;
        public List<BerserkerNode> nodePath = new List<BerserkerNode>();
        private float startTime;
        
        private float speed;

        private int minNodes = 3;
        private int maxNodes = 6;
        
        public override void OnStart()
        {
            base.OnStart();
            speed = enemyEntity.GetCustomDataValue<float>("entity", "speed");

            movementNodes = enemyEntity.Nodes;
            currentNode = finalPosition.Value.GetComponent<BerserkerNode>();
            GenerateNodePath();
            
            startTime = Time.time;

            taskStatus = TaskStatus.Running;
        }

        private void GenerateNodePath()
        {
            int amountOfNodes = Random.Range(minNodes, maxNodes) - 1; // account for pre-processing a node
            nodePath.Clear();

            BerserkerNode prevNode = null;
            BerserkerNode currNode = currentNode;
            nodePath.Add(currNode);
            while(amountOfNodes > 0)
            {
                BerserkerNode temp = currNode.GetRandomNode(prevNode);
                prevNode = currNode;
                currNode = temp;
                
                nodePath.Add(currNode);
                amountOfNodes--;
            }
            
            // duplicate first and last nodes for Catmull-Rom spline
            nodePath.Insert(0, nodePath[0]);
            nodePath.Add(nodePath[^1]);
        }
        
        private void GenerateRandomNodes(int amountOfNodes)
        {
            nodePath.Clear();
            
            HashSet<int> selectedIndices = new HashSet<int>();
            if(finalPosition.Value != null)
            {
                nodePath.Add(finalPosition.Value.GetComponent<BerserkerNode>());
            }
            
            while (selectedIndices.Count < amountOfNodes)
            {
                int randomIndex = Random.Range(0, movementNodes.Count - 1);
                if (selectedIndices.Add(randomIndex))
                {
                    nodePath.Add(movementNodes[randomIndex]);
                }

                if (selectedIndices.Count == amountOfNodes)
                {
                    nodePath.Insert(0, nodePath[0]);
                    nodePath.Add(nodePath[^1]);
                }
            }
        }
        
        public override TaskStatus OnUpdate()
        {
            Flying();
            return taskStatus;
        }

        public void Flying()
        {
            float length = 0f;
            for(int i = 0; i < nodePath.Count - 1; i++)
            {
                length += Vector3.Distance(nodePath[i].transform.position, nodePath[i + 1].transform.position);
            }

            length *= 0.9f;
            
            float duration = length / speed;
            
            int pointsCnt = nodePath.Count - 3;
            float currentTime = ((Time.time - startTime) / duration) % pointsCnt;
            int idx = Mathf.FloorToInt(currentTime) + 1;
            Vector3 position = MathFunctions.CatmullRomSplineInterp(
                nodePath[idx - 1].transform.position, 
                nodePath[idx].transform.position, 
                nodePath[idx + 1].transform.position, 
                nodePath[idx + 2].transform.position, 
                currentTime % 1.0f);
            
            transform.position = position;
            
            if(Vector3.Distance(nodePath[^1].transform.position, transform.position) < 0.1f)
            {
                finalPosition.Value = nodePath[^1].gameObject;
                taskStatus = TaskStatus.Success;
            }
        }
        
        public override void OnEnd()
        {
            base.OnEnd();
        }
    }
}