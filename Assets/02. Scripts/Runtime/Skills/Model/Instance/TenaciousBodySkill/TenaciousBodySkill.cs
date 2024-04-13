using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Player;

namespace _02._Scripts.Runtime.Skills.Model.Instance.TenaciousBodySkill {
	public class TenaciousBodySkill  : PassiveSkillEntity<TenaciousBodySkill>, ICanGetModel   {
		[field: ES3Serializable] 
		public override string EntityName { get; set; } = "TenaciousBodySkill";

		[ES3Serializable] private int alreadyAddedHealth = 0;
		[ES3Serializable] private float alreadyAddedHealthRecoverSpeed = 0;
		
		protected override void OnInitModifiers(int rarity) {
			
		}
		
		protected override void OnEntityStart(bool isLoadedFromSave) {
			base.OnEntityStart(isLoadedFromSave);
			if (isInInventory) {
				IPlayerEntity playerEntity = this.GetModel<IGamePlayerModel>().GetPlayer();
				playerEntity.RegisterOnModifyReceivedAddArmorAmount(OnModifyArmorAmount);
			}
		}

		public override void OnAddedToInventory(string playerUUID) {
			base.OnAddedToInventory(playerUUID);
			
			IPlayerEntity playerEntity = this.GetModel<IGamePlayerModel>().GetPlayer();
			playerEntity.RegisterOnModifyReceivedAddArmorAmount(OnModifyArmorAmount);
		}

		private float OnModifyArmorAmount(float n) {
			if (!isInHotBarSlot) return n;
			return 0;
		}


		public override bool Collectable { get; } = true;
		protected override string GetDescription(string defaultLocalizationKey) {
			string additionalDesc = "";
			if (GetLevel() >= 2) {
				float addedHealthRecoverSpeed = GetCustomPropertyOfCurrentLevel<float>("added_health_recover_speed");
				additionalDesc = "\n\n" + Localization.GetFormat("TenaciousBodySkill_desc2", addedHealthRecoverSpeed);
			}
			int addAmount = GetCustomPropertyOfCurrentLevel<int>("add_amount");
			return Localization.GetFormat(defaultLocalizationKey, addAmount, additionalDesc);
		}

		protected override void OnAddedToHotBar() {
			AddHealthOfLevel(GetRarity());
		}

		protected override void OnRemovedFromHotBar() {
			IPlayerEntity playerEntity = this.GetModel<IGamePlayerModel>().GetPlayer();
			playerEntity.ChangeMaxHealth(-alreadyAddedHealth);
			playerEntity.GetHealthRecoverSpeed().RealValue.Value -= alreadyAddedHealthRecoverSpeed;
			alreadyAddedHealth = 0;
			alreadyAddedHealthRecoverSpeed = 0;
		}

		protected override void OnUpgrade(int previousLevel, int level) {
			if (!isInHotBarSlot) {
				AddHealthOfLevel(level);
			}
		}
		
		private void AddHealthOfLevel(int level) {
			int addAmount = GetCustomPropertyWithLevel<int>("add_amount", level);
			int realAddAmount = addAmount - alreadyAddedHealth;
			if (realAddAmount < 0) return;
			
			alreadyAddedHealth = addAmount;
			IPlayerEntity playerEntity = this.GetModel<IGamePlayerModel>().GetPlayer();
			playerEntity.ChangeMaxHealth(realAddAmount);
			
			if(level >= 2) {
				float addedHealthRecoverSpeed = GetCustomPropertyWithLevel<float>("added_health_recover_speed", level);
				
				float realAddedArmorRecoverSpeed = addedHealthRecoverSpeed - alreadyAddedHealthRecoverSpeed;
				alreadyAddedHealthRecoverSpeed = addedHealthRecoverSpeed;

				playerEntity.GetHealthRecoverSpeed().RealValue.Value += realAddedArmorRecoverSpeed;
			}
		}

		public override void OnRemovedFromInventory() {
			base.OnRemovedFromInventory();
			IPlayerEntity playerEntity = this.GetModel<IGamePlayerModel>().GetPlayer();
			OnRemovedFromHotBar();
			playerEntity.UnRegisterOnModifyReceivedAddArmorAmount(OnModifyArmorAmount);
		}
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}

		public override void OnRecycled() {
			base.OnRecycled();
			alreadyAddedHealth = 0;
			alreadyAddedHealthRecoverSpeed = 0;
		}
	}
}