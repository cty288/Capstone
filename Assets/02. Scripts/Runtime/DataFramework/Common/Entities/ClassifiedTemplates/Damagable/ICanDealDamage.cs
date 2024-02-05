using System;
using System.Collections.Generic;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using UnityEngine;

namespace Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable {
	public interface ICanDealDamage : IBelongToFaction {
		
		void DoOnKillDamageable(IDamageable damageable) {
			OnKillDamageable(damageable);
			OnKillDamageableCallback?.Invoke(damageable);
			Debug.Log(this.GetType().Name + "kill " + damageable.GetType().Name);
			ParentDamageDealer?.DoOnKillDamageable(damageable);
		}
		
		void OnKillDamageable(IDamageable damageable);

		void DoOnDealDamage(IDamageable damageable, int damage) {
			OnDealDamage(damageable, damage);
			OnDealDamageCallback?.Invoke(damageable, damage);
			Debug.Log("[ICanDealDamageDebug]" +this.GetType().Name + "deal damage to " + damageable.GetType().Name + " with " + damage +
			          " damage.");
			ParentDamageDealer?.DoOnDealDamage(damageable, damage);
		}
		
		void OnDealDamage(IDamageable damageable, int damage);

		int DoModifyDamageCount(int damage) {
			int modifiedDamage = damage;
			if (OnModifyDamageCountCallbackList != null) {
				foreach (var onModifyDamageCount in OnModifyDamageCountCallbackList) {
					modifiedDamage = onModifyDamageCount(modifiedDamage);
				}
			}
			return ParentDamageDealer?.DoModifyDamageCount(modifiedDamage) ?? modifiedDamage;
		}

		void RegisterOnModifyDamageCount(Func<int, int> onModifyDamageCount) {
			OnModifyDamageCountCallbackList.Add(onModifyDamageCount);
		}

		void UnregisterOnModifyDamageCount(Func<int, int> onModifyDamageCount) {
			OnModifyDamageCountCallbackList.Remove(onModifyDamageCount);
		}

		HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; }

		void RegisterOnDealDamage(Action<IDamageable, int> onDealDamage) {
			OnDealDamageCallback += onDealDamage;
		}
		
		void UnregisterOnDealDamage(Action<IDamageable, int> onDealDamage) {
			OnDealDamageCallback -= onDealDamage;
		}
		
		Action<IDamageable, int> OnDealDamageCallback { get; protected set; }


		void RegisterOnKillDamageable(Action<IDamageable> onKillDamageable) {
			OnKillDamageableCallback += onKillDamageable;
		}
		
		void UnregisterOnKillDamageable(Action<IDamageable> onKillDamageable) {
			OnKillDamageableCallback -= onKillDamageable;
		}
		
		Action<IDamageable> OnKillDamageableCallback { get; protected set; }

		ICanDealDamage ParentDamageDealer { get; }


		Transform GetRootDamageDealerTransform() {
			ICanDealDamageViewController deepestVC = null;
			ICanDealDamage current = this;
			while (current != null) {
				if (current is ICanDealDamageViewController vc) {
					deepestVC = vc;
				}
				current = current.ParentDamageDealer;
			}

			return deepestVC?.GetTransform();
		}
		
		ICanDealDamage GetRootDamageDealer() {
			ICanDealDamage root = this;
			//string log = "[Tracing root damage dealer] Origin: " + this.GetType().Name + " -> \n ";
			while (root.ParentDamageDealer != null) {
				root = root.ParentDamageDealer;
				//log += root.GetType().Name + " -> \n ";
			}

			//Debug.Log(log);
			
			return root;
		}
		
		//public ICanDealDamageRootViewController RootViewController { get; }
	}
	
	public interface ICanDealDamageViewController : ICanDealDamage {
		public Transform GetTransform();
	}
	
}