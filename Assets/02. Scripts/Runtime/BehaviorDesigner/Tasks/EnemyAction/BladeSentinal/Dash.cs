using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _02._Scripts.Runtime.Utilities;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.Spawning;
using UnityEngine;
using UnityEngine.AI;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

public class Dash : EnemyAction<BladeSentinelEntity> {
    private GameObject model;
    private MoveBakeMesh mbm;
    private NavMeshAgent agent;
    private Animator animator;
    
    [SerializeField]
    private SharedGameObject player;

    private float minDistance;
    private float maxDistance;
    
    private bool dashPosFound = false;
    private Vector3 teleportLocation = Vector3.zero;
    private Vector3 beforeTeleportLocation = Vector3.zero;
    private float dashSpeed = 20f;
    private float minDashDistanceFromLastPos = 5f;
    private float dashFinishWaitTime = 0.5f;
    private float dashFinishWaitTimer = 0f;
    private TaskStatus status;
    //private Rigidbody rigidbody;
    public override void OnAwake() {
        base.OnAwake();
        model = gameObject.transform.Find("Pivot/Model").gameObject;
        mbm = gameObject.GetComponent<MoveBakeMesh>();
        agent = GetComponent<NavMeshAgent>();
        //rigidbody = GetComponent<Rigidbody>();
        animator = gameObject.GetComponentInChildren<Animator>(true);
    }

    public override void OnStart() {
        base.OnStart();
        minDistance = enemyEntity.GetCustomDataValue<float>("dash", "minDistance");
        maxDistance = enemyEntity.GetCustomDataValue<float>("dash", "maxDistance");
        dashSpeed = enemyEntity.GetCustomDataValue<float>("dash", "dashSpeed");
        minDashDistanceFromLastPos = enemyEntity.GetCustomDataValue<float>("dash", "minDistanceFromLastMove");
        dashFinishWaitTime = enemyEntity.GetCustomDataValue<float>("dash", "dashFinishWaitTime");
        
        dashFinishWaitTimer = 0f;
        dashPosFound = false;
        teleportLocation = Vector3.zero;
        beforeTeleportLocation = transform.position;
        //model.SetActive(false);
        mbm.enabled = false;
        agent.enabled = false;
        //rigidbody.isKinematic = true;
        status = TaskStatus.Running;
        TaskExecute();
    }

    public override void OnFixedUpdate() {
        base.OnFixedUpdate();
        if (!dashPosFound) {
            status = TaskStatus.Running;
            return;
        }
        
        if (Vector3.Distance(transform.position, teleportLocation) < 3f) {
            mbm.enabled = false;
            
            if(dashFinishWaitTimer < dashFinishWaitTime) {
                dashFinishWaitTimer += Time.fixedDeltaTime;
                status = TaskStatus.Running;
                return;
            }
            else {
                model.SetActive(true);
                //rigidbody.isKinematic = false;
                agent.enabled = true;
                status = TaskStatus.Success;
                return;
            }
        }
        else {
            transform.position += (teleportLocation - transform.position).normalized * dashSpeed * Time.fixedDeltaTime;
            status = TaskStatus.Running;
            return;
        }
    }

    public override TaskStatus OnUpdate() {
        return status;
    }

    protected virtual async Task TaskExecute() {
        int attempts = 5000;


        while (attempts > 0) {
            Vector3 randomSide = new Vector3(Random.insideUnitCircle.normalized.x, 0,
                Random.insideUnitCircle.normalized.y);
            NavMeshHit hit;
            
            Vector3 targetPos = player.Value.transform.position + 
                                randomSide * Random.Range(minDistance, maxDistance);

            NavMesh.SamplePosition(targetPos, out hit, 40f, NavMeshHelper.GetSpawnableAreaMask());

            Vector3 hitPos = hit.position;
            if (float.IsInfinity(hitPos.magnitude) || Vector3.Distance(hitPos, beforeTeleportLocation) < minDashDistanceFromLastPos) {
                continue;
            }
            
            
            NavMeshFindResult result = await (SpawningUtility.FindNavMeshSuitablePosition(gameObject,
                () => enemyViewController.SpawnSizeCollider,
                hit.position, 60, NavMeshHelper.GetSpawnableAreaMask(), default, 10, 3, attempts));
            
            
            

            attempts -= result.UsedAttempts;
                
            if (result.IsSuccess) {
                teleportLocation = result.TargetPosition;
                break;
            }
        }
        
        if (teleportLocation == Vector3.zero) {
            teleportLocation = beforeTeleportLocation;
        }else {
            animator.CrossFadeInFixedTime("Teleport", 0.1f);
            await UniTask.WaitForSeconds(1f, false, PlayerLoopTiming.Update,
                gameObject.GetCancellationTokenOnDestroyOrRecycle());
        }

        dashPosFound = true;
        mbm.enabled = true;
        model.SetActive(false);
        Debug.Log("Find Dash Pos at " + teleportLocation);
    }

    public override void OnDrawGizmos() {
        base.OnDrawGizmos();
        //draw target position
        if (dashPosFound) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(teleportLocation, 1f);

        }
     
    }

    public override void OnEnd() {
        base.OnEnd();
        //rigidbody.isKinematic = false;
    }
}
