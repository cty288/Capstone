using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;

namespace Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable {
	public interface ICanDealDamage : IBelongToFaction{
		void OnKillDamageable(IDamageable damageable);
	}
}