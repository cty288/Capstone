using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Inventory.Model;

namespace _02._Scripts.Runtime.Skills.Model.Instance.HighEndTechnologySkill {
	public class HighEndTechnologySkill: PassiveSkillEntity<HighEndTechnologySkill>, ICanGetModel, ICanRegisterEvent   {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "HighEndTechnologySkill";
		
		
		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			base.OnEntityStart(isLoadedFromSave);
			if (isInInventory) {
				this.RegisterEvent<OnInventoryItemAddedEvent>(OnInventoryItemAdded);
			}
		}

		public override void OnAddedToInventory(string playerUUID) {
			base.OnAddedToInventory(playerUUID);
			this.RegisterEvent<OnInventoryItemAddedEvent>(OnInventoryItemAdded);
		}

		public override int GetMaxRarity() {
			return 2;
		}

		private void OnInventoryItemAdded(OnInventoryItemAddedEvent e) {
			if(e.Item.AddedToInventoryBefore) return;
			
			if (e.Item is IWeaponPartsEntity weaponPartsEntity) {
				int level = GetCustomPropertyOfCurrentLevel<int>("level");
				if (weaponPartsEntity.GetRarity() < level && weaponPartsEntity.GetMaxRarity() >= level) {
					weaponPartsEntity.GetProperty<IRarityProperty>().RealValue.Value = level;

					this.SendEvent<OnAddItemPickIndicatorContent>(new OnAddItemPickIndicatorContent() {
						Content = Localization.GetFormat("HighEndTechnologySkill_hint",
							GetDisplayName(), weaponPartsEntity.GetDisplayName(), level)
					});
				}
			}
		}

		public override void OnRemovedFromInventory() {
			base.OnRemovedFromInventory();
			this.UnRegisterEvent<OnInventoryItemAddedEvent>(OnInventoryItemAdded);
		}

		public override bool Collectable { get; } = true;
		protected override string GetDescription(string defaultLocalizationKey) {
			int level = GetCustomPropertyOfCurrentLevel<int>("level");
			return Localization.GetFormat(defaultLocalizationKey, level);
		}

		protected override void OnUpgrade(int previousLevel, int level) {
			
		}

		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
}