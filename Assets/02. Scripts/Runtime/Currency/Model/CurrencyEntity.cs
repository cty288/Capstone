using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.Utilities.ConfigSheet;

namespace _02._Scripts.Runtime.Currency.Model {
	public enum CurrencyType {
		Combat,
		Time,
		Plant,
		Mineral
	}
	
	public interface ICurrencyEntity : IResourceEntity {
		public CurrencyType CurrencyType { get; }
		public int Amount { get; }
		
		public void InitCurrency(CurrencyType currencyType, int amount);
	}

	/// <summary>
	/// For pickable currency.
	/// </summary>
	public class CurrencyEntity : ResourceEntity <CurrencyEntity>, ICurrencyEntity{
		public override string EntityName {
			get => "CURRENCY_" + CurrencyType.ToString();
			set { }
		}
		
		[field: ES3Serializable]
		public CurrencyType CurrencyType { get; protected set; }
		
		[field: ES3Serializable]
		public int Amount { get; protected set; }

		public void InitCurrency(CurrencyType currencyType, int amount) {
			CurrencyType = currencyType;
			Amount = amount;
		}

		protected override ConfigTable GetConfigTable() {
			return null;
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		public override void OnRecycle() {
			
		}

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}

		protected override void OnInitModifiers(int rarity) {
		
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}

		protected override string OnGetDisplayNameBeforeFirstPicked(string originalDisplayName) {
			return originalDisplayName;
		}

		public override ResourceCategory GetResourceCategory() {
			return ResourceCategory.Currency;
		}

		public override string OnGroundVCPrefabName { get; } = null;
		public override bool Collectable { get; } = false;

		public override IResourceEntity GetReturnToBaseEntity() {
			return this;
		}
		
	}
}