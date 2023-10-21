using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;

namespace Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable {
	public interface ICanDealDamage : IBelongToFaction{
		void OnKillDamageable(IDamageable damageable);
		
		void OnDealDamage(IDamageable damageable, int damage);
		
		ICanDealDamageRootEntity RootDamageDealer { get; }
	}
	
	public interface ICanDealDamageRootEntity : ICanDealDamage, IEntity {
		
	}
}