using _02._Scripts.Runtime.ResourceCrafting.Models;
using _02._Scripts.Runtime.ResourceCrafting.Models.Build;
using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;

namespace _02._Scripts.Runtime.Skills {
	public interface ISkillSystem : ISystem {
		
	}
	public class SkillSystem : AbstractSystem, ISkillSystem {

		private static string[] initiallyPurchaseableSkillNames = new string[] {
			//"GrenadeSkill"
		};
		
		protected IResourceBuildModel buildModel;
		protected ISkillModel skillModel;
		
		protected override void OnInit() {
			buildModel = this.GetModel<IResourceBuildModel>();
			skillModel = this.GetModel<ISkillModel>();
			if (buildModel.IsFirstTimeCreated) {
				foreach (string skillName in initiallyPurchaseableSkillNames) {
					//ISkillEntity skillEntity = GetNewSkillEntity(skillName);
					buildModel.UnlockBuild(ResearchCategory.Skill, skillName, false);
				}
			}

			foreach (var skillEntity in skillModel.GetAllResources()) {
				skillEntity.OnGetSystems();
			}
		}
		
	}
}