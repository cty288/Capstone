using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Inventory.Model;

namespace _02._Scripts.Runtime.Skills.Model.Instance.BackpackSkill {
	public class BackpackSkill : PassiveSkillEntity<BackpackSkill>, ICanGetModel  {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "BackpackSkill";
		
		[field: ES3Serializable]
		private int actualAddedSlots = 0;

		private bool removed = false;
		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable { get; } = true;
		protected override string GetDescription(string defaultLocalizationKey) {
			if (GetLevel() < 4) {
				int count = GetCustomPropertyOfCurrentLevel<int>("count");
				return Localization.GetFormat(defaultLocalizationKey, count);
			}

			return Localization.Get("BackpackSkill_desc2");
		}

		protected override void OnUpgrade(int previousLevel, int level) {
			int addedSlots = GetCustomPropertyOfCurrentLevel<int>("count");
			IInventoryModel inventoryModel = this.GetModel<IInventoryModel>();
			inventoryModel.AddSlots(addedSlots, out int addedCount);
			actualAddedSlots += addedCount;
		}

		public override void OnRemovedFromInventory() {
			base.OnRemovedFromInventory();
			
			IInventoryModel inventoryModel = this.GetModel<IInventoryModel>();
			inventoryModel.RemoveSlots(actualAddedSlots);
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			base.OnEntityStart(isLoadedFromSave);
		}

		public override void OnAddedToInventory(string playerUUID) {
			base.OnAddedToInventory(playerUUID);
			IInventoryModel inventoryModel = this.GetModel<IInventoryModel>();
			int addedSlots = GetCustomPropertyOfCurrentLevel<int>("count");
			inventoryModel.AddSlots(addedSlots, out actualAddedSlots);
		}

		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
}