using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.Utilities.ConfigSheet;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Properties;

namespace _02._Scripts.Runtime.Skills.Model.Base {
	public interface ISkillEntity : IResourceEntity, IHaveCustomProperties, IHaveTags {
		
	}
	public abstract class SkillEntity<T>:  ResourceEntity<T>, ISkillEntity  where T : SkillEntity<T>, new()  {
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.SkillEntityConfigTable;
		}
		
		public override ResourceCategory GetResourceCategory() {
			return ResourceCategory.Skill;
		}

		protected override string OnGetDisplayNameBeforeFirstPicked(string originalDisplayName) {
			return originalDisplayName;
		}

		public override string OnGroundVCPrefabName { get; } = null;

		[field: ES3Serializable]
		public string InventoryVCPrefabName { get; } = "SkillInventoryVC";
	}
}