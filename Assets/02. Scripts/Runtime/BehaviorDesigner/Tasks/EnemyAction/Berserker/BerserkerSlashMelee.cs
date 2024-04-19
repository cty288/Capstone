using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Enemies.ViewControllers.Instances.Berserker;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class BerserkerSlashMelee : EnemyAction<BerserkerEntity> {
   private enum Phase
    {
        Stomp,
        Complete
    }

    private Phase _phase;
    
    private TaskStatus taskStatus;
    
    private float _progress;
    
    private float speed;

    private Vector3 _startPos;
    private Vector3 _targetPos;

    private Transform _player;

    private LayerMask mask = LayerMask.GetMask("Default", "Ground", "Wall");
    private SafeGameObjectPool pool;
    private SafeGameObjectPool slashPool;
    public SharedGameObject slashPrefab;
    public SharedGameObject stompImpact;
    public SharedGameObject burnVFX;
    private VisualEffect vfx;
    private SharedGameObject generatedEMPField;
    // **** Values to add to spreadsheet.
    private float _stompHeight = 50f;
    private float _stompTime = 0.75f;
    private float _readySpeed = 50f;

    public override void OnAwake() {
        base.OnAwake();
        slashPool = GameObjectPoolManager.Singleton.CreatePool(slashPrefab.Value, 5, 10);
        pool = GameObjectPoolManager.Singleton.CreatePool(stompImpact.Value, 2, 5);
    }

    public override void OnStart()
    {
        base.OnStart();
        _player = GetPlayer().transform;
        _phase = Phase.Stomp;
        speed = enemyEntity.GetCustomDataValue<float>("entity", "speed");
        generatedEMPField = (SharedGameObject) GetComponent<BehaviorTree>().GetVariable("GeneratedEMPField");
        _startPos = transform.position;
        _progress = 0;
        taskStatus = TaskStatus.Running;
        
        vfx = burnVFX.Value.GetComponent<VisualEffect>();
        vfx.Stop();
        SetupTargetPosition();
    }
    
    public override TaskStatus OnUpdate()
    {
        if(_phase == Phase.Stomp) {
            Stomp();
        }
        else {
            Stun();
        }
        return taskStatus;
    }

    // Fly above the player and hover after them for a while. Drop a range indicator.
    public void SetupTargetPosition() {
        _startPos = gameObject.transform.position;
        transform.rotation = Quaternion.LookRotation((_player.transform.position - _startPos).normalized);
        _targetPos = _player.transform.position - Vector3.up * (_player.transform.localScale.y / 2);
        if (NavMesh.SamplePosition(_targetPos, out NavMeshHit hit, 1000, NavMeshHelper.GetSpawnableAreaMask())) {
            _targetPos = hit.position;
        }
        vfx.Play();
    }

    // Crash down onto the range indicator.
    public void Stomp()
    {
        _progress += Time.deltaTime / _stompTime;
        transform.position = Vector3.Lerp(_startPos, _targetPos, _progress);
        //transform.rotation = Quaternion.LookRotation(Vector3.down, Quaternion.Euler(0, _progress * 360 * 10, 0) * Vector3.forward);

        if (_progress >= 1)
        {
            int damage = enemyEntity.GetCustomDataValue<int>("slash", "melee_damage");
            GameObject explosion = pool.Allocate();
            explosion.transform.position = this.gameObject.transform.position;
            explosion.GetComponent<IExplosionViewController>().
                Init(enemyEntity.CurrentFaction.Value, 
                    damage,4, gameObject,
                    gameObject.GetComponent<ICanDealDamage>());
            _progress = 0;
            vfx.Stop();
            
            
            
            float slashSpeed = enemyEntity.GetCustomDataValue<float>("slash", "slash_speed");
            float slashScaleMultiplier = enemyEntity.GetCustomDataValue<float>("slash", "scale_multiplier");
            int slashDamage = enemyEntity.GetCustomDataValue<int>("slash", "slash_damage");

            SpawnSlash(slashSpeed, slashScaleMultiplier, slashDamage);

            //StopEMPField();
            
            if (generatedEMPField.Value) {
                BerserkerEMPField empField = generatedEMPField.Value.GetComponent<BerserkerEMPField>();
                empField.DestroyField();
            }
            
            _phase = Phase.Complete;
        }
    }

    
    private void SpawnSlash(float slashSpeed, float slashScaleMultiplier, int slashDamage) {
        UnityEngine.GameObject b = slashPool.Allocate();
        b.transform.position = enemyViewController.GetTransform().position;
        b.transform.localScale = new Vector3(slashScaleMultiplier, slashScaleMultiplier, slashScaleMultiplier);


        b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
            slashDamage,
            gameObject, gameObject.GetComponent<ICanDealDamage>(), 50f);


        b.GetComponent<BerserkerSlashBullet>().SetData(slashSpeed,
            GetPlayer().transform.position - enemyViewController.GetTransform().position);
    }
    public void Stun()
    {
        _progress += Time.deltaTime * 2f;
        transform.rotation = Quaternion.LookRotation(_player.position - transform.position);

        if (_progress >= 1)
        {
            taskStatus = TaskStatus.Success;
        }
    }
    
    public override void OnEnd()
    {
        base.OnEnd();
    }
}
