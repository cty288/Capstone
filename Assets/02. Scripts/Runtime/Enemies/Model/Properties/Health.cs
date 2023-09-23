using Runtime.DataFramework.Properties;

namespace Runtime.Enemies.Model.Properties {
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

	/// <summary>
	/// Health property. Contains current and max health. <br/>
	/// Depends on <see cref="Rarity"/> property. <br/>
	/// </summary>
	public interface IHealthProperty : IProperty<HealthInfo>, ILoadFromConfigProperty {
		public int GetMaxHealth();
		
		public int GetCurrentHealth();
	}
	
	/// <summary>
	/// Health property. Contains current and max health. <br/>
	/// Depends on <see cref="Rarity"/> property. <br/>
	/// </summary>
	public class Health: AbstractLoadFromConfigProperty<HealthInfo>, IHealthProperty {
		
		public int GetMaxHealth() {
			return RealValue.Value.MaxHealth;
		}

		public int GetCurrentHealth() {
			return RealValue.Value.CurrentHealth;
		}

		

		protected override IPropertyDependencyModifier<HealthInfo> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.health;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity)};
		}
		
		
	}

}