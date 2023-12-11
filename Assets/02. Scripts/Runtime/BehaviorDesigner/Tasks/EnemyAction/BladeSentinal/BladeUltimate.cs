using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.Spawning;
using UnityEngine;

public class BladeUltimate : EnemyAction<BladeSentinelEntity> {
    private Animator animator;
    private TaskStatus taskStatus;

    private float bladeInterval;
    private float bladeCount;
    private float bladeWaitTime;
    private float bladeDamage;
    
    public SharedGameObject swordPrefab;
    private SafeGameObjectPool pool;
    
    [SerializeField] private float jumpHeight;
    public override void OnAwake() {
        base.OnAwake();
        animator = gameObject.GetComponentInChildren<Animator>(true);
        pool = GameObjectPoolManager.Singleton.CreatePool(swordPrefab.Value, 20, 50);
    }

    public override void OnStart() {
        base.OnStart();
        animator.CrossFadeInFixedTime("Ultimate_Jump", 0.2f);
		
        bladeInterval = enemyEntity.GetCustomDataValue<float>("honingBlades", "bladeInterval");
        bladeCount = enemyEntity.GetCustomDataValue<int>("honingBlades", "bladeCount");
        bladeWaitTime = enemyEntity.GetCustomDataValue<float>("honingBlades", "bladeWaitTime");
        bladeDamage = enemyEntity.GetCustomDataValue<int>("honingBlades", "bladeDamage");
		
        taskStatus = TaskStatus.Running;
    }

    public override TaskStatus OnUpdate() {
        return taskStatus;
    }

    public async UniTask SkillExecute() {
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Ultimate_Air"),
            PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycle());
        
        //Jump
        float duration = 1;
        float timeElapsed = 0;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = transform.position + new Vector3(0, jumpHeight, 0);
        while (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            await UniTask.Yield();
        }
        transform.position = targetPosition;
        animator.CrossFadeInFixedTime("Ultimate_Attack", 0.2f);
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Ultimate_End"), PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycle());
        //start spawning blades
        for (int i = 0; i < bladeCount; i++) {
            await UniTask.Delay((int) (bladeWaitTime * 1000), false, PlayerLoopTiming.Update,
                gameObject.GetCancellationTokenOnDestroyOrRecycle());
            //spawn blade
            UnityEngine.GameObject s = pool.Allocate();
        }
    }
}