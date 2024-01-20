using _02._Scripts.Runtime.ResourceCrafting.Models.Build;
using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;

namespace _02._Scripts.Runtime.Skills {
	public interface ISkillSystem : ISystem {
		public void UnlockPurchaseableSkill(string skillName);
		
		//public void RemoveSlot(PreparationSlot slot);
	}
	public class SkillSystem : AbstractSystem, ISkillSystem {

		private static string[] initiallyPurchaseableSkillNames = new string[] {
			//"GrenadeSkill"
		};
		
		protected IResourceBuildModel buildModel;
		
		protected override void OnInit() {
			buildModel = this.GetModel<IResourceBuildModel>();
			if (buildModel.IsFirstTimeCreated) {
				foreach (string skillName in initiallyPurchaseableSkillNames) {
					//ISkillEntity skillEntity = GetNewSkillEntity(skillName);
					buildModel.UnlockBuild(ResourceCategory.Skill, skillName);
				}
			}
		}
		
		protected ISkillEntity GetNewSkillEntity(string skillName) {
			return ResourceVCFactory.Singleton.SpawnNewResourceEntity(skillName, true, 1) as ISkillEntity;
		}

		public void UnlockPurchaseableSkill(string skillName) {
			buildModel.UnlockBuild(ResourceCategory.Skill, skillName);
		}
	}
}