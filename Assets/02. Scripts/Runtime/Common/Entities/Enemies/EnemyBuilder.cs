using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Base.Entity;
using _02._Scripts.Runtime.Common.Properties;
using MikroFramework.Pool;

namespace _02._Scripts.Runtime.Common.Entities.Enemies {
	public class EnemyBuilder<T> : EntityBuilder<T> where T : class, IEntity, new() {
		
		public EnemyBuilder() {
			CheckEntity();
		}
		public EnemyBuilder<T> SetDanger(int danger, IPropertyDependencyModifier<int> modifier = null) {
			SetProperty<int>(PropertyName.danger, danger, modifier);
			return this;
		}
		
		public EnemyBuilder<T> SetDangerModifier(IPropertyDependencyModifier<int> modifier = null) {
			SetModifier(PropertyName.danger, modifier);
			return this;
		}
		
		public EnemyBuilder<T> SetHealth(HealthInfo healthInfo, IPropertyDependencyModifier<HealthInfo> modifier = null) {
			SetProperty<HealthInfo>(PropertyName.health, healthInfo, modifier);
			return this;
		}
		
		public EnemyBuilder<T> SetHealthModifier(IPropertyDependencyModifier<HealthInfo> modifier = null) {
			SetModifier(PropertyName.health, modifier);
			return this;
		}

		public EnemyBuilder<T> SetTaste(params TasteType[] tasteTypes) {
			SetProperty<List<TasteType>>(PropertyName.taste, tasteTypes.ToList());
			return this;
		}
		
		public EnemyBuilder<T> SetVigiliance(float vigiliance, IPropertyDependencyModifier<float> modifier = null) {
			SetProperty<float>(PropertyName.vigiliance, vigiliance, modifier);
			return this;
		}
		
		public EnemyBuilder<T> SetVigilianceModifier(IPropertyDependencyModifier<float> modifier = null) {
			SetModifier(PropertyName.vigiliance, modifier);
			return this;
		}
		
		public EnemyBuilder<T> SetAttackRange(float attackRange, IPropertyDependencyModifier<float> modifier = null) {
			SetProperty<float>(PropertyName.attack_range, attackRange, modifier);
			return this;
		}
		
		public EnemyBuilder<T> SetAttackRangeModifier(IPropertyDependencyModifier<float> modifier = null) {
			SetModifier(PropertyName.attack_range, modifier);
			return this;
		}


		
		public EnemyBuilder<T> SetAllBasics(int danger, HealthInfo healthInfo, float vigiliance, float attackRange,
			params TasteType[] tasteTypes) {
			SetDanger(danger);
			SetHealth(healthInfo);
			SetVigiliance(vigiliance);
			SetAttackRange(attackRange);
			SetTaste(tasteTypes);
			return this;
		}

		public EnemyBuilder<T> SetAllBasicsModifiers(IPropertyDependencyModifier<int> dangerModifier, 
			IPropertyDependencyModifier<HealthInfo> healthInfoModifier, IPropertyDependencyModifier<float> vigilianceModifier, 
			IPropertyDependencyModifier<float> attackRangeModifier) {
			SetDangerModifier(dangerModifier);
			SetHealthModifier(healthInfoModifier);
			SetVigilianceModifier(vigilianceModifier);
			SetAttackRangeModifier(attackRangeModifier);
			return this;
		}
		public override void RecycleToCache() {
			SafeObjectPool<EnemyBuilder<T>>.Singleton.Recycle(this);
		}
		
		public static EnemyBuilder<T> Allocate(int rarity) {
			EnemyBuilder<T> target = SafeObjectPool<EnemyBuilder<T>>.Singleton.Allocate();
			target.SetProperty<int>(PropertyName.rarity, rarity);
			return target;
		}
	}
}