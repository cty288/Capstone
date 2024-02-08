using System;
using System.Collections.Generic;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using UnityEngine;

namespace Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable {
	public interface ICanDealDamage : IBelongToFaction {
		
		void DoOnKillDamageable(ICanDealDamage sourceDealer, IDamageable damageable) {
			OnKillDamageable(sourceDealer, damageable);
			OnKillDamageableCallback?.Invoke(sourceDealer, damageable);
			Debug.Log(this.GetType().Name + "kill " + damageable.GetType().Name);
			ParentDamageDealer?.DoOnKillDamageable(sourceDealer, damageable);
		}
		
		void OnKillDamageable(ICanDealDamage sourceDealer, IDamageable damageable);

		void DoOnDealDamage(ICanDealDamage sourceDealer, IDamageable damageable, int damage) {
			OnDealDamage(sourceDealer, damageable, damage);
			OnDealDamageCallback?.Invoke(sourceDealer, damageable, damage);
			Debug.Log("[ICanDealDamageDebug]" +this.GetType().Name + "deal damage to " + damageable.GetType().Name + " with " + damage +
			          " damage.");
			ParentDamageDealer?.DoOnDealDamage(sourceDealer, damageable, damage);
		}
		
		void OnDealDamage(ICanDealDamage sourceDealer, IDamageable damageable, int damage);

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

		void RegisterOnDealDamage(Action<ICanDealDamage, IDamageable, int> onDealDamage) {
			OnDealDamageCallback += onDealDamage;
		}
		
		void UnregisterOnDealDamage(Action<ICanDealDamage, IDamageable, int> onDealDamage) {
			OnDealDamageCallback -= onDealDamage;
		}
		
		Action<ICanDealDamage, IDamageable, int> OnDealDamageCallback { get; protected set; }


		void RegisterOnKillDamageable(Action<ICanDealDamage, IDamageable> onKillDamageable) {
			OnKillDamageableCallback += onKillDamageable;
		}
		
		void UnregisterOnKillDamageable(Action<ICanDealDamage, IDamageable> onKillDamageable) {
			OnKillDamageableCallback -= onKillDamageable;
		}
		
		Action<ICanDealDamage, IDamageable> OnKillDamageableCallback { get; protected set; }

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