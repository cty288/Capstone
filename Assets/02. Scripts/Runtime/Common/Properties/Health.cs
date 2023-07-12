using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;

namespace _02._Scripts.Runtime.Common.Properties {
	public struct HealthInfo {
		public int MaxHealth;
		public int CurrentHealth;
		
		public HealthInfo(int maxHealth, int currentHealth) {
			MaxHealth = maxHealth;
			CurrentHealth = currentHealth;
		}
		
		
		public static HealthInfo operator +(HealthInfo healthInfo, int value) {
			healthInfo.CurrentHealth += value;
			return healthInfo;
		}
		
		public static HealthInfo operator -(HealthInfo healthInfo, int value) {
			healthInfo.CurrentHealth -= value;
			return healthInfo;
		}

	}
	
	public interface IHealthProperty : IProperty<HealthInfo>{}
	public class Health: Property<HealthInfo>, IHealthProperty {
		
		public int GetMaxHealth() {
			return RealValue.Value.MaxHealth;
		}
		
		
		
		protected override IPropertyDependencyModifier<HealthInfo> GetDefautModifier() {
			return new HealthDefaultModifier();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.health;
		}

		public override PropertyName[] GetDependentProperties() {
			return new[] {PropertyName.rarity};
		}
		
		
	}
	
	public class HealthDefaultModifier : PropertyDependencyModifier<HealthInfo> {
		public override HealthInfo OnModify(HealthInfo propertyValue) {
			int rarityMultiplier = GetDependency<Rarity>().InitialValue * 5;
			propertyValue = new HealthInfo(propertyValue.MaxHealth * rarityMultiplier, propertyValue.CurrentHealth * rarityMultiplier);
			return propertyValue;
		}
	}
}