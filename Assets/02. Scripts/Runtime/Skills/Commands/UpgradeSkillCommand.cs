using System;
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
			ISkillModel skillModel = this.GetModel<ISkillModel>();
			IInventoryModel inventoryModel = this.GetModel<IInventoryModel>();

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
					currencyModel.RemoveCurrency(costKey, cost[costKey]);
				}
				inventoryModel.RemoveItem(originalSkill.UUID);
				skillModel.RemoveEntity(originalSkill.UUID);
				inventoryModel.AddItem(upgradedSkill);
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