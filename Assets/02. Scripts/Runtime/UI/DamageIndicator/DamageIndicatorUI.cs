using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Pool;
using Runtime.Player;
using Runtime.Temporary;
using UnityEngine;

public class DamageIndicatorUI : AbstractMikroController<MainGame> {
    [SerializeField] private GameObject damageIndicatorPrefab;
    //private SafeGameObjectPool damageIndicatorPool;
    private Transform playerTransform;
    private IPlayerEntity playerEntity;
    private Dictionary<DamageIndicator, Transform> damageIndicatorToTransformMap = new Dictionary<DamageIndicator, Transform>();
    private Dictionary<Transform, DamageIndicator> transformToDamageIndicatorMap = new Dictionary<Transform, DamageIndicator>();
    List<Transform> transformsToRemove = new List<Transform>();
    private void Awake() {
       // damageIndicatorPool = GameObjectPoolManager.Singleton.GetOrCreatePool(damageIndicatorPrefab);
        
        playerTransform = PlayerController.GetClosestPlayer(Camera.main.transform.position).transform;
        playerEntity = this.GetModel<IGamePlayerModel>().GetPlayer();
        this.RegisterEvent<OnPlayerTakeDamage>(OnPlayerTakeDamage).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void OnPlayerTakeDamage(OnPlayerTakeDamage e) {
        Transform targetTransform = e.HitData?.Attacker?.RootViewController?.GetTransform();
        if (!targetTransform || !targetTransform.gameObject.activeInHierarchy) {
            return;
        }

        float curHealth = playerEntity.GetCurrentHealth();
        float damagePower = Mathf.Max(0.1f, e.DamageTaken / curHealth);
        
        if (transformToDamageIndicatorMap.ContainsKey(targetTransform)) {
            transformToDamageIndicatorMap[targetTransform].UpdateDamage(damagePower);
        }
        else {
            DamageIndicator damageIndicator =
                GameObject.Instantiate(damageIndicatorPrefab).GetComponent<DamageIndicator>();
            Transform transform1;
            (transform1 = damageIndicator.transform).SetParent(transform);
            transform1.localPosition = Vector3.zero;
            transform1.localScale = Vector3.one;
            transform1.localRotation = Quaternion.identity;
            damageIndicator.UpdateDamage(damagePower);
            damageIndicator.RegisterFadeCompleteCallback(RemoveByIndicator);
            damageIndicatorToTransformMap.TryAdd(damageIndicator, targetTransform);
            transformToDamageIndicatorMap.TryAdd(targetTransform, damageIndicator);
        }
    }

    private void Update() {
        transformsToRemove.Clear();
        foreach (Transform targetTr in transformToDamageIndicatorMap.Keys) {
            if (!targetTr || !targetTr.gameObject.activeInHierarchy) {
                transformsToRemove.Add(targetTr);
                continue;
            }
            DamageIndicator damageIndicator = transformToDamageIndicatorMap[targetTr];
            Vector3 damageDirection = targetTr.position - playerTransform.position;
            Vector3 localDamageDirection = playerTransform.InverseTransformDirection(damageDirection);
            float angle = Mathf.Atan2(localDamageDirection.x, localDamageDirection.z) * Mathf.Rad2Deg;
            
            //lerp damage indicator rotation
            Quaternion targetRotation = Quaternion.Euler(0, 0, -angle);
            damageIndicator.transform.rotation =
                Quaternion.Lerp(damageIndicator.transform.rotation, targetRotation, Time.deltaTime * 30);
        }
        
        foreach (Transform transformToRemove in transformsToRemove) {
            RemoveByTransform(transformToRemove);
        }
    }

    private void RemoveByIndicator(DamageIndicator indicator) {
        if (!damageIndicatorToTransformMap.ContainsKey(indicator)) {
            return;
        }
        Transform targetTransform = damageIndicatorToTransformMap[indicator];
        damageIndicatorToTransformMap.Remove(indicator);
        transformToDamageIndicatorMap.Remove(targetTransform);
       // damageIndicatorPool.Recycle(indicator.gameObject);
    }
    
    private void RemoveByTransform(Transform targetTransform) {
        DamageIndicator damageIndicator = transformToDamageIndicatorMap[targetTransform];
        transformToDamageIndicatorMap.Remove(targetTransform);
        damageIndicatorToTransformMap.Remove(damageIndicator);
        //damageIndicatorPool.Recycle(damageIndicator.gameObject);
    }
}
