using System.Collections.Generic;
using System.Linq;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties;
using Runtime.Enemies.Model.Properties;

namespace Runtime.Enemies.Model.Builders {
	public class EnemyBuilder<T> : CreatureBuilder<EnemyBuilder<T>, T> where T : class, IEntity, new() {
		
		public EnemyBuilder() {
			CheckEntity();
		}
		public EnemyBuilder<T> SetDanger(int danger, IPropertyDependencyModifier<int> modifier = null) {
			SetProperty<int>(new PropertyNameInfo(PropertyName.danger), danger, modifier);
			return this;
		}
		
		public EnemyBuilder<T> SetDangerModifier(IPropertyDependencyModifier<int> modifier = null) {
			SetModifier(new PropertyNameInfo(PropertyName.danger), modifier);
			return this;
		}
		
		

		public EnemyBuilder<T> SetTaste(params TasteType[] tasteTypes) {
			
			SetProperty<List<TasteType>>(new PropertyNameInfo(PropertyName.taste), tasteTypes.ToList());
			return this;
		}
		



		
		public EnemyBuilder<T> SetAllBasics(int danger, HealthInfo healthInfo,
			params TasteType[] tasteTypes) {
			SetDanger(danger);
			SetHealth(healthInfo);
			SetTaste(tasteTypes);
			return this;
		}

		public EnemyBuilder<T> SetAllBasicsModifiers(IPropertyDependencyModifier<int> dangerModifier, 
			IPropertyDependencyModifier<HealthInfo> healthInfoModifier) {
			SetDangerModifier(dangerModifier);
			SetHealthModifier(healthInfoModifier);
			return this;
		}
		public override void RecycleToCache() {
			SafeObjectPool<EnemyBuilder<T>>.Singleton.Recycle(this);
		}
		
		public static EnemyBuilder<T> Allocate(int rarity) {
			EnemyBuilder<T> target = SafeObjectPool<EnemyBuilder<T>>.Singleton.Allocate();
			target.SetProperty<int>(new PropertyNameInfo(PropertyName.rarity), rarity);
			return target;
		}
	}
}