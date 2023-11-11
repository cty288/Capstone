using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;

namespace _02._Scripts.Runtime.Currency.Model {
	public interface ICurrencyModel : IModel {
		public BindableProperty<int> GetCurrencyAmountProperty(CurrencyType currencyType);
		
		public void AddCurrency(CurrencyType currencyType, int amount);
		
		public void RemoveCurrency(CurrencyType currencyType, int amount);
		
		public Dictionary<CurrencyType, int> GetCurrencyAmountDict();
	}
	
	public struct OnCurrencyAmountChangedEvent {
		public CurrencyType CurrencyType;
		public int Amount;
		public int CurrentAmount;
	}
	
	public class CurrencyModel : AbstractSavableModel, ICurrencyModel {
		
		[field: ES3Serializable]
		private Dictionary<CurrencyType, BindableProperty<int>> currencyAmountDict =
			new Dictionary<CurrencyType, BindableProperty<int>>();

		protected override void OnInit() {
			base.OnInit();
			if (IsFirstTimeCreated) {
				foreach (CurrencyType currencyType in System.Enum.GetValues(typeof(CurrencyType))) {
					currencyAmountDict.Add(currencyType, new BindableProperty<int>());
				}
			}
		}


		public BindableProperty<int> GetCurrencyAmountProperty(CurrencyType currencyType) {
			return currencyAmountDict[currencyType];
		}

		public void AddCurrency(CurrencyType currencyType, int amount) {
			currencyAmountDict[currencyType].Value += amount;
			this.SendEvent<OnCurrencyAmountChangedEvent>(new OnCurrencyAmountChangedEvent() {
				Amount = amount,
				CurrencyType = currencyType,
				CurrentAmount = currencyAmountDict[currencyType].Value
			});
		}

		public void RemoveCurrency(CurrencyType currencyType, int amount) {
			int beforeChangeAmount = currencyAmountDict[currencyType].Value;
			if (currencyAmountDict[currencyType].Value - amount < 0) {
				currencyAmountDict[currencyType].Value = 0;
			}
			else {
				currencyAmountDict[currencyType].Value -= amount;
			}

			int actualChangeAmount = beforeChangeAmount - currencyAmountDict[currencyType].Value;
			if (actualChangeAmount != 0) {
				this.SendEvent<OnCurrencyAmountChangedEvent>(new OnCurrencyAmountChangedEvent() {
					Amount = -actualChangeAmount,
					CurrencyType = currencyType,
					CurrentAmount = currencyAmountDict[currencyType].Value
				});
			}
			
		}

		public Dictionary<CurrencyType, int> GetCurrencyAmountDict() {
			Dictionary<CurrencyType, int> dict = new Dictionary<CurrencyType, int>();
			foreach (var pair in currencyAmountDict) {
				dict.Add(pair.Key, pair.Value.Value);
			}

			return dict;
		}
	}
}