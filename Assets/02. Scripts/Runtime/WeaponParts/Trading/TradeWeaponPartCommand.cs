using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Systems;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.Inventory.Model;

namespace _02._Scripts.Runtime.WeaponParts.Trading {
	public struct OnTradeWeaponPart {
		public IWeaponPartsEntity TradedPart;
		public IWeaponPartsEntity PreviewedPart;
		public bool IsExchange;
		public Dictionary<CurrencyType, int> PaidCost;
	
	}
	public class TradeWeaponPartCommand : AbstractCommand<TradeWeaponPartCommand> {
		private IWeaponPartsEntity tradedPart;
		private IWeaponPartsEntity previewedPart;
		private bool isExchange;
		private Dictionary<CurrencyType, int> paidCost;
		
		public static TradeWeaponPartCommand Allocate(IWeaponPartsEntity tradedPart, IWeaponPartsEntity previewedPart,
			bool isExchange, Dictionary<CurrencyType, int> paidCost) {
			TradeWeaponPartCommand command = SafeObjectPool<TradeWeaponPartCommand>.Singleton.Allocate();
			
			command.tradedPart = tradedPart;
			command.previewedPart = previewedPart;
			command.isExchange = isExchange;
			command.paidCost = paidCost;
			
			return command;
		}

		protected override void OnExecute() {
			ICurrencySystem currencySystem = this.GetSystem<ICurrencySystem>();
			IWeaponPartsSystem weaponPartsSystem = this.GetSystem<IWeaponPartsSystem>();
			IInventorySystem inventorySystem = this.GetSystem<IInventorySystem>();
			
			if (isExchange) {
				weaponPartsSystem.RemoveCurrentLevelPurchaseableParts(tradedPart);
				inventorySystem.AddItem(tradedPart);
			}else {
				tradedPart.Upgrade(1);
			}
			
			foreach (KeyValuePair<CurrencyType, int> cost in paidCost) {
				currencySystem.RemoveCurrency(cost.Key, cost.Value);
			}
			
			this.SendEvent<OnTradeWeaponPart>(new OnTradeWeaponPart() {
				TradedPart = tradedPart,
				PreviewedPart = previewedPart,
				IsExchange = isExchange,
				PaidCost = paidCost
			});
		}
	}
}