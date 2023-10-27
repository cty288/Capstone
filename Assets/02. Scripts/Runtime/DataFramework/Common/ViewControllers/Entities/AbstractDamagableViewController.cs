using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.Utilities;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.DataFramework.ViewControllers.Entities {
	
	public interface IDamageableViewController : IEntityViewController {
		public IDamageable DamageableEntity { get; }
	}
	
	public interface ICanDealDamageViewController : IEntityViewController {
		public ICanDealDamage CanDealDamageEntity { get; }
	}
	
	/// <summary>
	/// An abstract view controller for entities that can take damage (have health)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TEntityModel"></typeparam>
	public abstract class AbstractDamagableViewController<T> : AbstractBasicEntityViewController<T>, IHurtResponder, IDamageableViewController
		where T : class, IHaveCustomProperties, IHaveTags, IDamageable{
		
		[Header("Hurtresponder_Info")]
		private List<HurtBox> hurtBoxes = new List<HurtBox>();
		
		[Header("Damage Number")]
		[SerializeField] protected bool showDamageNumber = true;
		//[SerializeField] private DamageNumberInfo damageNumberInfo;
		
		protected override void OnStart() {
			base.OnStart();
			BoundEntity.RegisterOnTakeDamage(OnTakeDamage).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			BoundEntity.RegisterOnHeal(OnHeal).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			hurtBoxes = new List<HurtBox>(GetComponentsInChildren<HurtBox>(true));
			foreach (HurtBox hurtBox in hurtBoxes) {
				hurtBox.HurtResponder = this;
			}
		}

		private void OnHeal(int healamount, int currenthealth, IBelongToFaction healer) {
			OnEntityHeal(healamount, currenthealth, healer);
		}

		private void OnTakeDamage(int damage, int currenthealth, ICanDealDamage damagedealer, [CanBeNull] HitData hitData) {
			OnEntityTakeDamage(damage, currenthealth, damagedealer);
			if (showDamageNumber && (hitData == null || hitData.ShowDamageNumber)) {
				DamageNumberHUD.Singleton.SpawnHUD(hitData?.HitPoint ?? transform.position, damage,
					hitData?.Hurtbox?.DamageMultiplier > 1f);
			}
			if (currenthealth <= 0) {
				OnEntityDie(damagedealer);
				
			}
		}

		/// <summary>
		/// When the entity dies, this method will be called
		/// </summary>
		/// <param name="damagedealer"></param>
		protected abstract void OnEntityDie(ICanDealDamage damagedealer);

		/// <summary>
		/// When the entity takes damage, this method will be called
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="currenthealth"></param>
		/// <param name="damagedealer">The dealer of the damage. You can access its faction from it</param>
		protected abstract void OnEntityTakeDamage(int damage, int currenthealth, ICanDealDamage damagedealer);
		
		/// <summary>
		/// When the entity is healed, this method will be called
		/// </summary>
		/// <param name="heal"></param>
		/// <param name="currenthealth"></param>
		/// <param name="healer"></param>
		protected abstract void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer);

		public BindableProperty<Faction> CurrentFaction => BoundEntity.CurrentFaction;
		public bool CheckHurt(HitData data) {
			// Debug.Log("check hurt: " + BoundEntity.CheckCanTakeDamage(data.Attacker));
			// return BoundEntity.CheckCanTakeDamage(data.Attacker);
			return true;
		}

		public virtual void HurtResponse(HitData data) {
			// Debug.Log("I AM HURTING");
			BoundEntity.TakeDamage(data.Damage,data.Attacker, data);
		}

		public IDamageable DamageableEntity => BoundEntity;
	}
}