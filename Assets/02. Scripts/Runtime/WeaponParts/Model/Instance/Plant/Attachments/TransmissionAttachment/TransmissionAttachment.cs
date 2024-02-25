using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.PlantBuff;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Properties;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Plant.Attachments.TransmissionAttachment {
	public class TransmissionAttachment : WeaponPartsEntity<TransmissionAttachment,TransmissionAttachmentBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "TransmissionAttachment";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}
		
		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class TransmissionAttachmentBuff : WeaponPartsBuff<TransmissionAttachment,
		TransmissionAttachmentBuff>, ICanGetSystem {
		[field: ES3Serializable] public override float TickInterval { get; protected set; } = -1;

		private Dictionary<IDamageable, Transform> targetTransforms = new Dictionary<IDamageable, Transform>();
		

		public override void OnInitialize() {
			RegisterWeaponBuildBuffEvent<OnHackedBuffAdded>(OnHackedBuffAdded);
		}

		private void OnHackedBuffAdded(OnHackedBuffAdded e) {
			e.Target.RegisterOnDie(OnTargetDie);
			targetTransforms.TryAdd(e.Target, e.TargetTransform);
		}

		private void OnTargetDie(ICanDealDamage killer, IDamageable target, HitData hit) {
			if (IsRecycled || !targetTransforms.ContainsKey(target) || !weaponEntity.IsHolding) return;
			
			Transform targetTransform = targetTransforms[target];
			targetTransforms.Remove(target);
			
			if (!targetTransform) {
				return;
			}

			if (weaponEntity.GetMainBuildType() != CurrencyType.Plant) {
				return;
			}
			
			IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();
			if(!buffSystem.ContainsBuff<HackedBuff>(target, out IBuff buff)) {
				return;
			}
			HackedBuff hackedBuff = buff as HackedBuff;
			float range = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("range");
			Dictionary<IEnemyEntity, Transform> enemies = new Dictionary<IEnemyEntity, Transform>();

			LayerMask mask = LayerMask.GetMask("Default");
			Collider[] colliders = Physics.OverlapSphere(targetTransform.position, range, mask);
			
			foreach (var collider in colliders) {
				if(!collider.attachedRigidbody) continue;
				IEnemyViewController enemy = collider.attachedRigidbody.GetComponent<IEnemyViewController>();
				if (enemy != null && enemy.EnemyEntity != null) {
					enemies.TryAdd(enemy.EnemyEntity, collider.transform);
				}
			}
						
			//apply buff to all enemies
			foreach (var enemyKVP in enemies) {
				IEnemyEntity enemy = enemyKVP.Key;
				Transform enemyTransform = enemyKVP.Value;
				if (enemy != target) {
					PlantBuff.AddHackedBuff(weaponEntity, enemyTransform, enemy, hackedBuff.MaxDuration,
						hackedBuff.DamagePerTick,
						hackedBuff.DamageMultiplier, hackedBuff.IsSuddenDeathBuff, hackedBuff.SuddenDeathBuffDamage);
				}
			}
		}


		public override void OnStart() {
			base.OnStart();
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			
		}

		public override void OnRecycled() {
			base.OnRecycled();
			UnRegisterWeaponBuildBuffEvent<OnHackedBuffAdded>(OnHackedBuffAdded);
			foreach (var entity in targetTransforms.Keys) {
				entity.UnRegisterOnDie(OnTargetDie);
			}
			targetTransforms.Clear();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.Get("TransmissionAttachment_desc"));
				})
			};
		}
	}
}