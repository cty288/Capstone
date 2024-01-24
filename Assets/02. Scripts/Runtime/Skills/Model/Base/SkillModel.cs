using System.Collections.Generic;
using _02._Scripts.Runtime.Skills.Model.Builders;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;

namespace _02._Scripts.Runtime.Skills.Model.Base {
	public interface ISkillModel : IGameResourceModel<ISkillEntity> {
		SkillBuilder<T> GetSkillBuilder<T>(int rarity, bool addToModelOnceBuilt = true)
			where T : class, ISkillEntity, new();
		
		/*List<PreparationSlot> GetPurchasableSkillSlots();
		
		void UnlockSkill(ISkillEntity skillEntity);
		
		void RemoveSlot(PreparationSlot slot);*/
	}
	public class SkillModel : GameResourceModel<ISkillEntity>, ISkillModel {
		[field: ES3Serializable]
		private List<PreparationSlot> purchasableSkillSlots = new List<PreparationSlot>();
		
		
		public SkillBuilder<T> GetSkillBuilder<T>(int rarity, bool addToModelOnceBuilt = true) where T : class, ISkillEntity, new() {
			SkillBuilder<T> builder = entityBuilderFactory.GetBuilder<SkillBuilder<T>, T>(rarity);
			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}

			return builder;
		}

		public List<PreparationSlot> GetPurchasableSkillSlots() {
			return purchasableSkillSlots;
		}
		

		public void UnlockSkill(ISkillEntity skillEntity) {
			PreparationSlot slot = new PreparationSlot();
			slot.TryAddItem(skillEntity);
			purchasableSkillSlots.Add(slot);
		}

		public void RemoveSlot(PreparationSlot slot) {
			purchasableSkillSlots.Remove(slot);
		}
	}
}