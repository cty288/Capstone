using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _02._Scripts.Runtime.Utilities;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Spawning;
using UnityEngine;
using UnityEngine.AI;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction{

    public class Teleport : EnemyAction
    {
        private Vector3 teleportLocation;
        public GameObject vfx;
        public SharedGameObject player;
        public bool finished= false;

        private bool finishCalculatePath = false;
        BoxCollider spawnsizeCollider() => gameObject.GetComponent<ICreatureViewController>().SpawnSizeCollider;
        private NavMeshAgent navAgent;
        public override void OnStart()
        {
            base.OnStart();
            if(vfx == null)
            {
                Transform teleport = this.transform.Find("Teleport");
                vfx = teleport.gameObject;
            }
            vfx.SetActive(true);
            vfx.GetComponent<ParticleSystem>().Play();
            finished = false;
            navAgent = gameObject.GetComponent<NavMeshAgent>();
            navAgent.enabled = false;
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
            float height = spawnsizeCollider().bounds.size.y;
            Vector3 targetPosition = transform.position - new Vector3(0, height, 0);
            while (timeElapsed < duration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPosition;

            int attempts = 50;


            while (attempts > 0) {
                Vector3 randomSide = new Vector3(Random.insideUnitCircle.normalized.x, 0,
                    Random.insideUnitCircle.normalized.y);
                NavMeshHit hit;
                Vector3 targetPos = player.Value.transform.position + randomSide * 5f;
                NavMesh.SamplePosition(targetPos,out hit,40f,NavMeshHelper.GetSpawnableAreaMask());

                Vector3 hitPos = hit.position;
                if (float.IsInfinity(hitPos.magnitude)) {
                    continue;
                }
            
                Task<NavMeshFindResult> task = (SpawningUtility.FindNavMeshSuitablePosition(gameObject, spawnsizeCollider,
                    hit.position, 60, NavMeshHelper.GetSpawnableAreaMask(), default, 10, 3, attempts)).AsTask();


                yield return new WaitUntil(() => task.IsCompleted);

                attempts -= task.Result.UsedAttempts;
                
                if (task.Result.IsSuccess) {
                    teleportLocation = task.Result.TargetPosition;
            
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
                    break;
                }
            }
           
            
            finished = true;
           
        }

         public override void OnEnd() {
             base.OnEnd();
             vfx.SetActive(false);
             navAgent.enabled = true;
         }
    }

}
