using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.Spawning;
using UnityEngine;
using UnityEngine.AI;

public class SentinalMeleeAttack : EnemyAction<BladeSentinelEntity> {
    [SerializeField] private GameObject meleeBlade;
    
    
    private GameObject model;
    private MoveBakeMesh mbm;
    private NavMeshAgent agent;
    private Animator animator;
    private SharedGameObject player;
    
    private float minDashDistanceFromLastPos = 5f;
    
    private float meleeWaitTime = 0.5f;
    private float dashSpeed = 20f;
    
    private float dashFinishWaitTimer = 0f;
    private TaskStatus status;
    public override void OnAwake() {
        base.OnAwake();
        model = gameObject.transform.Find("Pivot/Model").gameObject;
        mbm = gameObject.GetComponent<MoveBakeMesh>();
        agent = GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponentInChildren<Animator>(true);
    }


    public override void OnStart() {
        base.OnStart();
        player = GetPlayer();
        dashSpeed = enemyEntity.GetCustomDataValue<float>("melee", "dashSpeed");
        meleeWaitTime = enemyEntity.GetCustomDataValue<float>("melee", "meleeWaitTime");
        
        dashFinishWaitTimer = 0f;
        mbm.enabled = false;
        agent.enabled = false;
        status = TaskStatus.Running;
        
        TaskExecute();
    }

    public override TaskStatus OnUpdate() {
        Transform playerTr = GetPlayer().transform;
        Vector3 direction = playerTr.position - gameObject.transform.position;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        //look rotation.y - 48
		
        lookRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
		
        gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, lookRotation, Time.deltaTime * 30f);
        
        return status;
    }

    protected virtual async UniTask TaskExecute() {
        int attempts = 10;

        //distance : 2;
        model.SetActive(true);
        meleeBlade.SetActive(true);
        
        Vector3 teleportLocation = Vector3.zero;
        animator.CrossFadeInFixedTime("Melee_Prepare", 0.1f);
        
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Melee_Hold"),
            PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
        
        
        while (attempts > 0) {
            //2 units away from player faced direction
            Vector3 targetPos = player.Value.transform.position + player.Value.transform.forward * 2f;
            NavMeshHit hit;

            bool res = NavMesh.SamplePosition(targetPos, out hit, 40f, NavMeshHelper.GetSpawnableAreaMask());

            Vector3 hitPos = hit.position;
            if (!res || float.IsInfinity(hitPos.magnitude)) {
                await UniTask.Yield();
                continue;
            }

            NavMeshFindResult result = await (SpawningUtility.FindNavMeshSuitablePosition(gameObject,
                () => enemyViewController.SpawnSizeCollider,
                hit.position, 60, NavMeshHelper.GetSpawnableAreaMask(), default, 10, 3, attempts));

            attempts -= result.UsedAttempts;

            if (result.IsSuccess && Vector3.Distance(result.TargetPosition, player.Value.transform.position) > 2f){
                teleportLocation = result.TargetPosition;
                break;
            }
            else {
                attempts--;
            }
        }

        if (teleportLocation == Vector3.zero) {
            status = TaskStatus.Failure;
            return;
        }

        

        mbm.enabled = true;
        model.SetActive(false);

        //dash to location
        //while (Vector3.Distance(transform.position, teleportLocation) > 0.1f) {
          //  transform.position += (teleportLocation - transform.position).normalized * dashSpeed * Time.deltaTime;
            //await UniTask.Yield(PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycle());
        //}
        float dashTime = Vector3.Distance(transform.position, teleportLocation) / dashSpeed;
        bool dashFinish = false;
        transform.DOMove(teleportLocation, dashTime).SetEase(Ease.Linear).OnComplete(() => {
            dashFinish = true;
        });

        await UniTask.WaitUntil(() => dashFinish, PlayerLoopTiming.Update,
            gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

        mbm.enabled = false;
        model.SetActive(true);
        animator.CrossFadeInFixedTime("Melee_Hold", 0f);

        await UniTask.WaitForSeconds(meleeWaitTime, false, PlayerLoopTiming.Update,
            gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

        animator.CrossFadeInFixedTime("Melee_Release", 0.1f);
       
        
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),
            PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

        
        status = TaskStatus.Success;
    }

    public override void OnEnd() {
        base.OnEnd();
        meleeBlade.SetActive(false);
        agent.enabled = true;
    }
}
