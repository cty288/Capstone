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
        
        private List<GameObject> movementNodes;
        private List<GameObject> points = new List<GameObject>();
        private float startTime;
        
        private float speed = 1.0f;
        
        public override void OnStart()
        {
            base.OnStart();
            movementNodes = enemyEntity.Nodes;
            GenerateRandomNodes(Random.Range(2, 4));
            startTime = Time.time;

            taskStatus = TaskStatus.Running;
            // SkillExecute();
        }
        
        private void GenerateRandomNodes(int amountOfNodes)
        {
            points.Clear();
            
            HashSet<int> selectedIndices = new HashSet<int>();
            if(finalPosition.Value != null)
            {
                points.Add(finalPosition.Value);
            }
            
            while (selectedIndices.Count < amountOfNodes)
            {
                int randomIndex = Random.Range(0, movementNodes.Count - 1);
                if (selectedIndices.Add(randomIndex))
                {
                    points.Add(movementNodes[randomIndex]);
                }

                if (selectedIndices.Count == amountOfNodes)
                {
                    points.Insert(0, points[0]);
                    points.Add(points[^1]);
                }
            }
        }
        
        public override TaskStatus OnUpdate()
        {
            Flying();
            return taskStatus;
        }

        // public async UniTask SkillExecute()
        // {
        // }

        public void Flying()
        {
            int pointsCnt = points.Count - 3;
            float currentTime = ((Time.time - startTime) * speed) % pointsCnt;
            int idx = Mathf.FloorToInt(currentTime) + 1;
            Vector3 position = MathFunctions.CatmullRomSplineInterp(points[idx - 1].transform.position, points[idx].transform.position, points[idx + 1].transform.position, points[idx + 2].transform.position, currentTime % 1.0f);
            
            transform.position = position;
            
            if(Vector3.Distance(points[^1].transform.position, transform.position) < 0.1f)
            {
                finalPosition.Value = points[^1];
                taskStatus = TaskStatus.Success;
            }
        }
        
        public override void OnEnd()
        {
            base.OnEnd();
        }
    }
}