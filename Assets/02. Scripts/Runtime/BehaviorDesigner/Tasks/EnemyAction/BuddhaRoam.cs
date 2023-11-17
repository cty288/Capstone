using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using _02._Scripts.Runtime.Utilities;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies;
using Runtime.Enemies.Model;
using Runtime.Spawning;
using UnityEngine;
using UnityEngine.AI;


namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class BuddhaRoam : EnemyAction<Boss1Entity>
    {
        public NavMeshAgent navAgent;
        private Transform playerTrans;
        public GameObject pivot;
        private float initialRotationSpeed = 0.0f;
        private float maxRotationSpeed = 0.0f;
        private Vector3 targetPos;
        float pathfindTimeOutTime = 8f;
        float roamRange = 8f;
        private float timer = 0f;
        private NavMeshPath path;
        private Vector3 finalPosition;
        BoxCollider spawnsizeCollider() => gameObject.GetComponent<ICreatureViewController>().SpawnSizeCollider;

        public override void OnStart()
        {
            navAgent = gameObject.GetComponent<Boss1>().agent;
            playerTrans = GetPlayer().transform;
            timer = 0f;
            base.OnStart();
            Vector3 randomSide = new Vector3(Random.insideUnitCircle.normalized.x, 0,
                Random.insideUnitCircle.normalized.y);
            Vector3 targetPos = playerTrans.position + randomSide * Random.Range(2f,12f);
            path = new NavMeshPath();

            NavMeshHit hit;
            
            navAgent.isStopped = false;
            NavMesh.SamplePosition(targetPos,out hit,40f,NavMeshHelper.GetSpawnableAreaMask());
            int used;
            Quaternion quaternion;
            finalPosition = SpawningUtility.FindNavMeshSuitablePosition(spawnsizeCollider,
                hit.position, 60, NavMeshHelper.GetSpawnableAreaMask(), null, 10, 3, 50, out used,out quaternion
            );
            NavMesh.CalculatePath(transform.position, finalPosition, NavMeshHelper.GetSpawnableAreaMask(), path);
            //navAgent.speed = 5f;
            navAgent.SetPath(path);

        }

        public override TaskStatus OnUpdate()
        {
            
            if (Vector3.Distance(finalPosition,transform.position)<=1f)
            {
                return TaskStatus.Success;
            }

            timer += Time.deltaTime;
            if (timer > pathfindTimeOutTime&&Vector3.Distance(transform.position, finalPosition) >= 10f)
            {
                navAgent.isStopped = true;
                return TaskStatus.Failure;
            }

            if (Vector3.Distance(transform.position, playerTrans.position) >= 30f)
            {
                navAgent.isStopped = true;
                return TaskStatus.Failure;
            }
            
             
            return TaskStatus.Running;
            //maxRotationSpeed = 260;
            //pivot.transform.Rotate(new Vector3(1,1,1)* maxRotationSpeed * Time.deltaTime);
            return base.OnUpdate();
        }
    }

}
