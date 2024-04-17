using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using Runtime.Enemies.ViewControllers.Instances.Berserker;
using Runtime.Player;
using Runtime.Temporary;
using UnityEngine;

public class BerserkerEMPField : AbstractMikroController<MainGame> {
   [SerializeField] private Transform followTarget;
   [SerializeField] private float maxTime = 10;
   
   private float followSpeed = 5f;
   private IPlayerEntity playerEntity;
   private IBuffSystem buffSystem;
   private bool isPlayerInField = false;
   private bool isDestroying = false;

   private void Awake() {
      buffSystem = this.GetSystem<IBuffSystem>();
      this.Delay(maxTime, DestroyField);
   }

   public void SetFollowTarget(Transform target, float speed, float scaleMultiplier) {
      followTarget = target;
      playerEntity = target.GetComponent<PlayerController>().GetPlayerEntity();
      followSpeed = speed;
      transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);
      AddBuff();
      isPlayerInField = true;
   }

   private void Update() {
      if (followTarget) {
         transform.position = Vector3.Slerp(transform.position, followTarget.position, followSpeed * Time.deltaTime);
      }
   }

   private void OnTriggerEnter(Collider other) {
      if (isDestroying) {
         return;
      }
      if (followTarget && other.attachedRigidbody.gameObject == followTarget.gameObject) {
         AddBuff();
         isPlayerInField = true;
      }
   }

   private void OnTriggerExit(Collider other) {
      if (followTarget && other.attachedRigidbody.gameObject == followTarget.gameObject) {
         RemoveBuff();
         isPlayerInField = false;
      }
   }
   
   public void AddBuff() {
      Debug.Log("Buff Added");
      buffSystem.AddBuff(playerEntity, playerEntity,
         LockWeaponsBuff.Allocate(playerEntity, playerEntity));
         
      buffSystem.AddBuff(playerEntity, playerEntity,
         LockActiveSkillsBuff.Allocate(playerEntity, playerEntity));
   }

   public void RemoveBuff() {
      buffSystem.RemoveBuff<LockWeaponsBuff>(playerEntity);
      buffSystem.RemoveBuff<LockActiveSkillsBuff>(playerEntity);
   }

   public void DestroyField() {
      if(isDestroying || !this) return;
      isDestroying = true;
      ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
      foreach (var particle in particles) {
         var module = particle.main; 
         module.loop = false;
      }
      this.Delay(3f, () => {
         if (this) {
            Destroy(gameObject);
         }
         
      });

      RemoveBuff();
   }
}
