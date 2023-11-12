using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;

namespace _02._Scripts.Runtime.Currency.Model {
	public interface ICurrencyModel : IModel {
		public int GetCurrencyAmountProperty(CurrencyType currencyType);
		
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
		private Dictionary<CurrencyType, int> currencyAmountDict =
			new Dictionary<CurrencyType, int>();

		protected override void OnInit() {
			base.OnInit();
			if (IsFirstTimeCreated) {
				foreach (CurrencyType currencyType in System.Enum.GetValues(typeof(CurrencyType))) {
					currencyAmountDict.Add(currencyType, new BindableProperty<int>());
				}
			}
		}


		public int GetCurrencyAmountProperty(CurrencyType currencyType) {
			return currencyAmountDict[currencyType];
		}

		public void AddCurrency(CurrencyType currencyType, int amount) {
			currencyAmountDict[currencyType] += amount;
			this.SendEvent<OnCurrencyAmountChangedEvent>(new OnCurrencyAmountChangedEvent() {
				Amount = amount,
				CurrencyType = currencyType,
				CurrentAmount = currencyAmountDict[currencyType]
			});
		}

		public void RemoveCurrency(CurrencyType currencyType, int amount) {
			int beforeChangeAmount = currencyAmountDict[currencyType];
			if (currencyAmountDict[currencyType] - amount < 0) {
				currencyAmountDict[currencyType] = 0;
			}
			else {
				currencyAmountDict[currencyType] -= amount;
			}

			int actualChangeAmount = beforeChangeAmount - currencyAmountDict[currencyType];
			if (actualChangeAmount != 0) {
				this.SendEvent<OnCurrencyAmountChangedEvent>(new OnCurrencyAmountChangedEvent() {
					Amount = -actualChangeAmount,
					CurrencyType = currencyType,
					CurrentAmount = currencyAmountDict[currencyType]
				});
			}
			
		}

		public Dictionary<CurrencyType, int> GetCurrencyAmountDict() {
			return currencyAmountDict;
		}
	}
}