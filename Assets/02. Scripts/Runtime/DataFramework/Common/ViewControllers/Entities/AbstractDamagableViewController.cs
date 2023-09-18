﻿using System.Collections.Generic;
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
	
	/// <summary>
	/// An abstract view controller for entities that can take damage (have health)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TEntityModel"></typeparam>
	public abstract class AbstractDamagableViewController<T> : AbstractBasicEntityViewController<T>, IHurtResponder
		where T : class, IHaveCustomProperties, IHaveTags, IDamageable{
		
		[Header("Hurtresponder_Info")]
		private List<HurtBox> hurtBoxes = new List<HurtBox>();
		
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

		private void OnTakeDamage(int damage, int currenthealth, IBelongToFaction damagedealer) {
			OnEntityTakeDamage(damage, currenthealth, damagedealer);
			
			if (currenthealth <= 0) {
				OnEntityDie(damagedealer);
			}
		}

		/// <summary>
		/// When the entity dies, this method will be called
		/// </summary>
		/// <param name="damagedealer"></param>
		protected abstract void OnEntityDie(IBelongToFaction damagedealer);
		
		/// <summary>
		/// When the entity takes damage, this method will be called
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="currenthealth"></param>
		/// <param name="damagedealer">The dealer of the damage. You can access its faction from it</param>
		protected abstract void OnEntityTakeDamage(int damage, int currenthealth, IBelongToFaction damagedealer);
		
		/// <summary>
		/// When the entity is healed, this method will be called
		/// </summary>
		/// <param name="heal"></param>
		/// <param name="currenthealth"></param>
		/// <param name="healer"></param>
		protected abstract void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer);

		public BindableProperty<Faction> CurrentFaction => BoundEntity.CurrentFaction;
		public bool CheckHurt(HitData data) {
			return BoundEntity.CheckCanTakeDamage(data.Attacker);
		}

		public virtual void HurtResponse(HitData data) {
			BoundEntity.TakeDamage(data.Damage,data.Attacker);
		}
	}
}