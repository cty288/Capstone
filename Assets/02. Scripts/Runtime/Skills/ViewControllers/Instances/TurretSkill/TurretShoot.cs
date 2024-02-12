using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using UnityEngine;

public class TurretShoot : Action {
    public float ShootLastTime = 10f;
    public SharedGameObject Target;
    public SharedFloat ShootInterval;
    
    public Transform shootPos;
    public GameObject muzzleFlash;

    private float shootLastTimer;
    private float shootTimer;
    private SafeGameObjectPool flashPool;

    public override void OnAwake() {
        base.OnAwake();
        flashPool = GameObjectPoolManager.Singleton.CreatePool(muzzleFlash, 50, 100);
    }

    public override void OnStart() {
        base.OnStart();
        shootLastTimer = 0;
    }

    public override TaskStatus OnUpdate() {
        if(Target.Value == null || Target.Value.activeSelf == false) {
            return TaskStatus.Success;
        }
        
        shootLastTimer += Time.deltaTime;
        if(shootLastTimer >= ShootLastTime) {
            return TaskStatus.Success;
        }
      
        shootTimer += Time.deltaTime;
        if(shootTimer >= ShootInterval.Value) {
            shootTimer = 0;
            Shoot();
            Debug.Log("Turret Shoot!!");
        }
      
        return TaskStatus.Running;
    }

    private void Shoot() {
        muzzleFlash = flashPool.Allocate();
        muzzleFlash.transform.position = shootPos.position;
        muzzleFlash.transform.rotation = shootPos.rotation;
    }
}

