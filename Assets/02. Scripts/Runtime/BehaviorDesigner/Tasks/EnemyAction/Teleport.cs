using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Spawning;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction{

    public class Teleport : EnemyAction
    {
        private Vector3 teleportLocation;

        public SharedGameObject player;
        public bool finished= false;


        BoxCollider spawnsizeCollider() => gameObject.GetComponent<ICreatureViewController>().SpawnSizeCollider;
        public override void OnStart()
        {
            base.OnStart();
            
            finished = false;
            
            StartCoroutine(StarTele());
        }

        
        public override TaskStatus OnUpdate()
        {
            if(finished)
                return TaskStatus.Success;
            else
            {
                return TaskStatus.Running;
            }
        }

        IEnumerator StarTele()
        {
            float duration = 1;
            float timeElapsed = 0;
            Vector3 startPosition = transform.position;
            float height = spawnsizeCollider().size.y;
            Vector3 targetPosition = transform.position - new Vector3(0, height, 0);
            while (timeElapsed < duration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPosition;

            Vector3 randomSide = new Vector3(Random.insideUnitCircle.normalized.x, 0,
                Random.insideUnitCircle.normalized.y);
            NavMeshHit hit;
            Vector3 targetPos = player.Value.transform.position + randomSide * 5f;
            NavMesh.SamplePosition(targetPos,out hit,40f,NavMeshHelper.GetSpawnableAreaMask());
            int used = 0;
            Quaternion quaternion;

            teleportLocation = SpawningUtility.FindNavMeshSuitablePosition(spawnsizeCollider,
                hit.position, 60, NavMeshHelper.GetSpawnableAreaMask(), null, 10, 3, 50, out used,out quaternion
            );
           /* while(float.IsInfinity(teleportLocation.magnitude)) {


            }*/
            timeElapsed = 0;
            transform.position = teleportLocation - new Vector3(0, height, 0) ;
            targetPosition = teleportLocation;
            startPosition = transform.position;
            while (timeElapsed < duration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPosition;
            finished = true;

        }
    }

}
