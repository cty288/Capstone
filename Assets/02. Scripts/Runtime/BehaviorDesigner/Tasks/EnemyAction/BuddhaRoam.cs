using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
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


        public override void OnStart()
        {
            navAgent = gameObject.GetComponent<Boss1>().agent;
            playerTrans = GetPlayer().transform;
            base.OnStart();
            
        }

        public override TaskStatus OnUpdate()
        {
            maxRotationSpeed = 260;
            pivot.transform.Rotate(new Vector3(1,1,1)* maxRotationSpeed * Time.deltaTime);
            return base.OnUpdate();
        }
    }

}
