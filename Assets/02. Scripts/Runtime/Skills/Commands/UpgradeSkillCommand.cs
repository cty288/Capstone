using System;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.GameResources;
using Runtime.Inventory.Model;
using Runtime.Weapons.ViewControllers.Base;

namespace _02._Scripts.Runtime.Skills.Commands {
	public class UpgradeSkillCommand : AbstractCommand<UpgradeSkillCommand> {
		private ISkillEntity originalSkill;
		private int level;
		private Action<ISkillEntity> onSkillUpgradeSuccessCallback;
		protected override void OnExecute() {
			ISkillEntity upgradedSkill =
				ResourceVCFactory.Singleton.SpawnNewResourceEntity(originalSkill.EntityName, true, level) as ISkillEntity;
			ICurrencyModel currencyModel = this.GetModel<ICurrencyModel>();
			ICurrencySystem currencySystem = this.GetSystem<ICurrencySystem>();
			ISkillModel skillModel = this.GetModel<ISkillModel>();
			IInventorySystem inventorySystem = this.GetSystem<IInventorySystem>();

			var cost = upgradedSkill.GetSkillUpgradeCostOfCurrentLevel();
			
			bool hasEnoughCurrency = true;
			foreach (CurrencyType costKey in cost.Keys) {
				if (!currencyModel.HasEnoughCurrency(costKey, cost[costKey])) {
					hasEnoughCurrency = false;
					break;
				}
			}

			if (hasEnoughCurrency) {
				foreach (CurrencyType costKey in cost.Keys) {
					currencySystem.RemoveCurrency(costKey, cost[costKey]);
				}
				inventorySystem.RemoveItem(originalSkill);
				skillModel.RemoveEntity(originalSkill.UUID);
				inventorySystem.AddItem(upgradedSkill);
				onSkillUpgradeSuccessCallback?.Invoke(upgradedSkill);
			}
			else {
				skillModel.RemoveEntity(upgradedSkill.UUID);
			}
		}
		
		
		public UpgradeSkillCommand() {
			
		}
		
		public static UpgradeSkillCommand Allocate(ISkillEntity originalSkill, int level, Action<ISkillEntity> onSkillUpgradeSuccessCallback) {
			UpgradeSkillCommand command = SafeObjectPool<UpgradeSkillCommand>.Singleton.Allocate();
			command.originalSkill = originalSkill;
			command.level = level;
			command.onSkillUpgradeSuccessCallback = onSkillUpgradeSuccessCallback;
			return command;
		}
	}
}