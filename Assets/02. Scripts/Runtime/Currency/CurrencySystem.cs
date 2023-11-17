using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Commands;
using _02._Scripts.Runtime.Player.Commands;
using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

namespace _02._Scripts.Runtime.Currency {
	public interface ICurrencySystem : ISystem {
		public float GetCurrencyToMoneyConversionRate(CurrencyType currencyType);
		
		public void AddCurrency(CurrencyType currencyType, int amount);
		
		public void RemoveCurrency(CurrencyType currencyType, int amount);
		
		public void RemoveMoney(int amount);
	}
	
	public struct OnMoneyAmountChangedEvent {
		public int Amount;
		public int CurrentAmount;
	}
	
	
	public class CurrencySystem : AbstractSystem, ICurrencySystem {
		private ICurrencyModel currencyModel;
		
		private Dictionary<CurrencyType, float> currencyToMoneyConversionRate = new Dictionary<CurrencyType, float>() {
		};
		protected override void OnInit() {
			this.RegisterEvent<OnSkillUsed>(OnSkillUsed);
			currencyModel = this.GetModel<ICurrencyModel>();

			//loop all currency types
			foreach (CurrencyType currencyType in Enum.GetValues(typeof(CurrencyType))) {
				float rate =
					float.Parse(
						ConfigDatas.Singleton.GlobalDataTable.Get($"MONEY_{currencyType.ToString()}", "Value1"));

				currencyToMoneyConversionRate.Add(currencyType, rate);
			}
			
			
			this.RegisterEvent<OnReturnToBase>(OnReturnToBase);
			this.RegisterEvent<OnPlayerRespawn>(OnPlayerRespawn);
		}

		

		private void OnSkillUsed(OnSkillUsed e) {
			Dictionary<CurrencyType, int> costs = e.skillEntity.GetSkillUseCostOfCurrentLevel();
			foreach (KeyValuePair<CurrencyType, int> cost in costs) {
				RemoveCurrency(cost.Key, cost.Value);
			}
		}

		public float GetCurrencyToMoneyConversionRate(CurrencyType currencyType) {
			return currencyToMoneyConversionRate[currencyType];
		}

		public void AddCurrency(CurrencyType currencyType, int amount) {
			if (amount == 0) {
				return;
			}
			currencyModel.AddCurrency(currencyType, amount);
			SendCurrencyChangedEvent(currencyType, amount, currencyModel.GetCurrencyAmountProperty(currencyType), false);
		}

		public void RemoveCurrency(CurrencyType currencyType, int amount) {
			int removedAmount = currencyModel.RemoveCurrency(currencyType, amount);
			if (removedAmount != 0) {
				SendCurrencyChangedEvent(currencyType, removedAmount, currencyModel.GetCurrencyAmountProperty(currencyType), false);
			}
		}

		public void RemoveMoney(int amount) {
			int removedAmount = currencyModel.RemoveMoney(amount);
			if (removedAmount != 0) {
				this.SendEvent<OnMoneyAmountChangedEvent>(new OnMoneyAmountChangedEvent() {
					Amount = removedAmount,
					CurrentAmount = currencyModel.Money.Value
				});
			}
		}


		private void OnReturnToBase(OnReturnToBase e) {
			foreach (CurrencyType currencyType in Enum.GetValues(typeof(CurrencyType))) {
				int amount = currencyModel.GetCurrencyAmountProperty(currencyType);
				if(amount <= 0) continue;
				currencyModel.RemoveCurrency(currencyType, amount);
				int moneyAmount = Mathf.RoundToInt(amount * currencyToMoneyConversionRate[currencyType]);
				currencyModel.AddMoney(moneyAmount);
				SendCurrencyChangedEvent(currencyType, amount, currencyModel.GetCurrencyAmountProperty(currencyType),
					true, moneyAmount);
			}
		}
		
		private void OnPlayerRespawn(OnPlayerRespawn obj) {
			foreach (CurrencyType currencyType in Enum.GetValues(typeof(CurrencyType))) {
				int amount = currencyModel.GetCurrencyAmountProperty(currencyType);
				if(amount <= 0) continue;
				currencyModel.RemoveCurrency(currencyType, amount);
			}
		}

		private void SendCurrencyChangedEvent(CurrencyType currencyType, int amount, int currentAmount, bool isTransferToMoney, int moneyAmount = 0) {
			this.SendEvent<OnCurrencyAmountChangedEvent>(new OnCurrencyAmountChangedEvent() {
				Amount = amount,
				CurrencyType = currencyType,
				CurrentAmount = currentAmount,
				IsTransferToMoney = isTransferToMoney,
				TransferAmount = moneyAmount
			});
		}
	}
}