using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using UnityEngine;

namespace Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable {
	public interface ICanDealDamage : IBelongToFaction{
		void OnKillDamageable(IDamageable damageable);
		
		void OnDealDamage(IDamageable damageable, int damage);
		
		ICanDealDamageRootEntity RootDamageDealer { get; }
		
		public ICanDealDamageRootViewController RootViewController { get; }
	}
	
	public interface ICanDealDamageRootEntity : ICanDealDamage, IEntity {
		
	}
	
	public interface ICanDealDamageRootViewController : ICanDealDamageViewController, IEntityViewController {
		public Transform GetTransform();
	}
}