using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;
using Runtime.GameResources;
using Runtime.Inventory.Model;

namespace _02._Scripts.Runtime.Skills {
	public interface ISkillSystem : ISystem {
		public void UnlockPurchaseableSkill(string skillName);
		
		public void RemoveSlot(PreparationSlot slot);
	}
	public class SkillSystem : AbstractSystem, ISkillSystem {

		private static string[] initiallyPurchaseableSkillNames = new string[] {
			"GrenadeSkill"
		};
		
		protected ISkillModel skillModel;
		
		protected override void OnInit() {
			skillModel = this.GetModel<ISkillModel>();
			if (skillModel.IsFirstTimeCreated) {
				foreach (string skillName in initiallyPurchaseableSkillNames) {
					ISkillEntity skillEntity = GetNewSkillEntity(skillName);
					skillModel.UnlockSkill(skillEntity);
				}
			}
		}
		
		protected ISkillEntity GetNewSkillEntity(string skillName) {
			return ResourceVCFactory.Singleton.SpawnNewResourceEntity(skillName, true, 1) as ISkillEntity;
		}

		public void UnlockPurchaseableSkill(string skillName) {
			ISkillEntity skillEntity = GetNewSkillEntity(skillName);
			skillModel.UnlockSkill(skillEntity);
		}

		public void RemoveSlot(PreparationSlot slot) {
			skillModel.RemoveSlot(slot);
		}
	}
}