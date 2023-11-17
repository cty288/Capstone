using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using _02._Scripts.Runtime.Utilities;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.Enemies;
using Runtime.Enemies.Model;
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

        public override void OnStart()
        {
            navAgent = gameObject.GetComponent<Boss1>().agent;
            playerTrans = GetPlayer().transform;
            base.OnStart();
            targetPos = playerTrans.position + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            navAgent.SetDestination(targetPos);

        }

        public override TaskStatus OnUpdate()
        {
            NavMeshPath path = new NavMeshPath();

            NavMesh.CalculatePath(transform.position, playerTrans.position, NavMeshHelper.GetSpawnableAreaMask(), path);
            if (Vector3.Distance(transform.position, targetPos) <= 1f)
            {
                //maxRotationSpeed = 0;
                return TaskStatus.Success;
            }


            if (path.status != NavMeshPathStatus.PathComplete)
            {
                return TaskStatus.Failure;
            }
             
            return TaskStatus.Running;
            //maxRotationSpeed = 260;
            //pivot.transform.Rotate(new Vector3(1,1,1)* maxRotationSpeed * Time.deltaTime);
            return base.OnUpdate();
        }
    }

}
