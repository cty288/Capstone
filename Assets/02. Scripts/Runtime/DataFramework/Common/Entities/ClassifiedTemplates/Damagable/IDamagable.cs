using MikroFramework.BindableProperty;
using MikroFramework.Event;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies.Model.Properties;

namespace Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable {
	
	
	/// <summary>
	/// Delegate for taking damage <br />
	/// Note that damage is the valid damage, meaning that if the real damage > current health, the valid damage will be current health; else the valid damage will be the real damage
	/// </summary>
	public delegate void OnTakeDamage(int damage, int currentHealth, IBelongToFaction damageDealer);
	
	/// <summary>
	/// Delegate for healing <br />
	/// Note that heal amount is the valid heal amount, meaning that if the real heal amount + current health > max health, the valid heal amount will be max health - current health; else the valid heal amount will be the real heal amount
	/// </summary>
	public delegate void OnHeal(int healAmount, int currentHealth, IBelongToFaction healer);
	
	
	public class OnTakeDamageUnRegister : IUnRegister {
		public IDamagable Damagable { get; set; }

		public  OnTakeDamage OnEvent { get; set; }

		public OnTakeDamageUnRegister(IDamagable damagable, OnTakeDamage onEvent) {
			Damagable = damagable;
			OnEvent = onEvent;
		}

		public void UnRegister() {
			Damagable.UnRegisterOnTakeDamage(OnEvent);
		}

	}
	
	public class OnHealUnRegister : IUnRegister {
		public IDamagable Damagable { get; set; }

		public  OnHeal OnEvent { get; set; }

		public OnHealUnRegister(IDamagable damagable, OnHeal onEvent) {
			Damagable = damagable;
			OnEvent = onEvent;
		}

		public void UnRegister() {
			Damagable.UnRegisterOnHeal(OnEvent);
		}

	}
	
	
	public interface IDamagable : IHaveCustomProperties, IBelongToFaction {
		
		/// <summary>
		/// Get the health property of the entity
		/// </summary>
		public IHealthProperty HealthProperty { get; }
		
		/// <summary>
		/// Get the max health of the entity
		/// </summary>
		public int GetMaxHealth();
		
		/// <summary>
		/// Get the current health of the entity
		/// </summary>
		
		public int GetCurrentHealth();
		
		/// <summary>
		/// Deal damage to the entity
		/// </summary>
		/// <param name="damage">the amount of damage</param>
		/// <param name="damageDealer">the damage dealer entity</param>
		
		public void TakeDamage(int damage, IBelongToFaction damageDealer);


		/// <summary>
		/// Register the event when the entity takes damage
		/// </summary>
		/// <param name="onTakeDamage"></param>
		/// <returns></returns>
		public IUnRegister RegisterOnTakeDamage(OnTakeDamage onTakeDamage);
		
		
		/// <summary>
		/// Unregister the event when the entity takes damage
		/// </summary>
		/// <param name="unRegister"></param>
		public void UnRegisterOnTakeDamage(OnTakeDamage onTakeDamage);
		
		/// <summary>
		/// Heal the entity
		/// </summary>
		/// <param name="healAmount">The amount of heal</param>
		/// <param name="healer">Healer</param>
		public void Heal(int healAmount, IBelongToFaction healer);
		
		
		/// <summary>
		/// Register the event when the entity is healed
		/// </summary>
		/// <param name="onHeal"></param>
		/// <returns></returns>
		public IUnRegister RegisterOnHeal(OnHeal onHeal);
		
		
		/// <summary>
		/// Unregister the event when the entity is healed
		/// </summary>
		/// <param name="unRegister"></param>
		public void UnRegisterOnHeal(OnHeal onHeal);
		
		/// <summary>
		/// If the entity is invincible
		/// </summary>
		
		public BindableProperty<bool> IsInvincible { get; }
	}
}