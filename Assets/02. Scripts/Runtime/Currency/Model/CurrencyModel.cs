using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;

namespace _02._Scripts.Runtime.Currency.Model {
	public interface ICurrencyModel : IModel {
		public int GetCurrencyAmountProperty(CurrencyType currencyType);
		
		public void AddCurrency(CurrencyType currencyType, int amount);
		
		public int RemoveCurrency(CurrencyType currencyType, int amount);
		
		public Dictionary<CurrencyType, int> GetCurrencyAmountDict();
		
		public bool HasEnoughCurrency(CurrencyType currencyType, int amount) {
			return GetCurrencyAmountProperty(currencyType) >= amount;
		}
		public BindableProperty<int> Money { get; }

		public void AddMoney(int amount) {
			Money.Value += amount;
		}
		
		public int RemoveMoney(int amount) {
			int beforeChangeAmount = Money.Value;
			if (Money.Value - amount < 0) {
				Money.Value = 0;
			}
			else {
				Money.Value -= amount;
			}

			int actualChangeAmount = beforeChangeAmount - Money.Value;
			

			return -actualChangeAmount;
		}
	}
	
	public struct OnCurrencyAmountChangedEvent {
		public CurrencyType CurrencyType;
		public int Amount;
		public int CurrentAmount;
		public bool IsTransferToMoney;
		public int TransferAmount;
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
			
		}

		public int RemoveCurrency(CurrencyType currencyType, int amount) {
			int beforeChangeAmount = currencyAmountDict[currencyType];
			if (currencyAmountDict[currencyType] - amount < 0) {
				currencyAmountDict[currencyType] = 0;
			}
			else {
				currencyAmountDict[currencyType] -= amount;
			}

			int actualChangeAmount = beforeChangeAmount - currencyAmountDict[currencyType];
			

			return -actualChangeAmount;

		}

		public Dictionary<CurrencyType, int> GetCurrencyAmountDict() {
			return currencyAmountDict;
		}

		[field: ES3Serializable]
		public BindableProperty<int> Money { get; private set; } = new BindableProperty<int>(0);
	}
}