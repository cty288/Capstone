using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Skills.Model.Instance.TurretSkill;
using _02._Scripts.Runtime.Skills.ViewControllers.Instances.TurretSkill;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using UnityEngine;
using UnityEngine.VFX;

public class TurretShoot : Action {
    public float ShootLastTime = 10f;
    public SharedGameObject Target;
    public SharedFloat ShootInterval;
    
    public Transform shootPos;
    public GameObject muzzleFlash;

    private float shootLastTimer;
    private float shootTimer;
    private SafeGameObjectPool flashPool;
    private LayerMask layer;
    
    private HitScan hitScan;
    private TurretEntity turretEntity;
    public VisualEffect[] bulletVFX;
    
    private Animator animator;
    public override void OnAwake() {
        base.OnAwake();
        flashPool = GameObjectPoolManager.Singleton.CreatePool(muzzleFlash, 50, 100);
        layer = LayerMask.GetMask("Default", "Hurtbox", "Ground", "Wall");
        animator = gameObject.GetComponentInChildren<Animator>();
    }

    public override void OnStart() {
        base.OnStart();
        shootLastTimer = 0;
        turretEntity = GetComponent<TurretViewController>().Entity;

        hitScan = new HitScan(turretEntity, Faction.Friendly, new List<VisualEffect>(bulletVFX), null, true);
        animator.SetBool("isShooting", true);

    }

    public override TaskStatus OnUpdate() {
        if(Target.Value == null || Target.Value.activeInHierarchy == false) {
            return TaskStatus.Success;
        }

        if (turretEntity.Ammo.Value <= 0) {
            return TaskStatus.Failure;
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
        
        HitDetectorInfo hitDetectorInfo = new HitDetectorInfo
        {
            camera = null,
            layer = layer,
            launchPoint = shootPos,
            weapon = null,
            direction = shootPos.forward,
            startPoint = shootPos.position
        };

        hitScan.CheckHit(hitDetectorInfo, turretEntity.GetCustomDataValue<int>("data", "damage"),
            new Collider[] {gameObject.GetComponent<Collider>()});
        
        turretEntity.UseAmmo();
    }

    public override void OnEnd() {
        base.OnEnd();
        animator.SetBool("isShooting", false);
    }
}

