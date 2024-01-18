using System;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Skills.Model.Builders;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Base {
	
	public interface ISkillViewController : IResourceViewController, ICanDealDamageViewController {
		ISkillEntity SkillEntity { get; }
	}
	
	
	
	public abstract class AbstractInHandSkillViewController<T> :
		AbstractPickableInHandResourceViewController<T>, ISkillViewController  where T : class, ISkillEntity, new() {
		
		private ISkillModel skillModel;

		protected override void Awake() {
			base.Awake();
			skillModel = this.GetModel<ISkillModel>();
		}

		public override IResourceEntity OnBuildNewPickableResourceEntity(bool setRarity, int rarity) {
			if(skillModel == null) {
				skillModel = this.GetModel<ISkillModel>();
			}

			SkillBuilder<T> builder = skillModel.GetSkillBuilder<T>(1);
			if (setRarity) {
				builder.SetProperty(new PropertyNameInfo(PropertyName.rarity), rarity);
			}

			return OnInitSkillEntity(builder);
		}

		protected abstract IResourceEntity OnInitSkillEntity(SkillBuilder<T> builder);


		public override void OnItemScopePressed() {
			
		}


		public override void OnItemScopeReleased() {
			
		}

		protected void UseSkill(Action onUseSuccess) {
			inventorySystem.ForceUpdateCurrentHotBarSlotCanSelect();
			if (BoundEntity == null) {
				return;
			}
			
			BoundEntity.UseSkill();
			onUseSuccess?.Invoke();
			inventorySystem.ForceUpdateCurrentHotBarSlotCanSelect();
		}

		public override void OnStartHold(GameObject ownerGameObject) {
			base.OnStartHold(ownerGameObject);
			if(ownerGameObject.TryGetComponent<ICanDealDamageViewController>(out var damageDealer)) {
				BoundEntity.SetOwner(damageDealer.CanDealDamageEntity);
			}
		}

		public ISkillEntity SkillEntity => BoundEntity;
		public ICanDealDamage CanDealDamageEntity => BoundEntity;
	}
}