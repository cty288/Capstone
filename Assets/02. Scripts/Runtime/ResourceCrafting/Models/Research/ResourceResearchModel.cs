using System.Collections.Generic;
using Framework;
using Runtime.GameResources.Model.Base;
using Runtime.RawMaterials.Model.Base;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

namespace _02._Scripts.Runtime.ResourceCrafting.Models {
	public interface IResourceResearchModel : ISavableModel {
		
		public int GetLevelIfExpAdded(ResourceCategory category,
			IHaveExpResourceEntity[] researchedResources, out ResearchLevelInfo[] addedResearchLevelInfos, out int researchDays);
		
		public int GetLevelIfExpAdded(ResourceCategory category,
			int totalExp, out ResearchLevelInfo[] addedResearchLevelInfos, out int researchDays);

		public int AddExp(ResourceCategory category, int exp, out ResearchLevelInfo[] addedResearchLevelInfos);

	}
	public class ResourceResearchModel : AbstractSavableModel, IResourceResearchModel {
		public int expResearchPerDay = 7;
		[ES3Serializable] 
		private Dictionary<ResourceCategory, ResourceResearchGroup> resourceResearchGroups;

		protected override void OnInit() {
			base.OnInit();
			if (IsFirstTimeCreated) {
				expResearchPerDay =
					int.Parse(ConfigDatas.Singleton.GlobalDataTable.Get<string>("EXP_RESEARCH_PER_DAY", "Value1"));

				LoadResourceGroupsFromConfig(ResourceCategory.Skill, ConfigDatas.Singleton.ResearchSkillsConfigTable);
			}
		}


		protected void LoadResourceGroupsFromConfig(ResourceCategory category, ConfigTable table) {
			int index = 0;
			List<ResearchLevelInfo> researchLevelInfos = new List<ResearchLevelInfo>();
			
			while (table.HasEntity(index.ToString())) {
				int totalExp = table.Get<int>(index.ToString(), "exp");
				string entityName = table.Get<string>(index.ToString(), "EntityName");
				researchLevelInfos.Add(new ResearchLevelInfo(totalExp, entityName));
				index++;
			}

			resourceResearchGroups.Add(category, new ResourceResearchGroup(researchLevelInfos.ToArray()));
		}

		public int GetLevelIfExpAdded(ResourceCategory category,
			IHaveExpResourceEntity[] researchedResources, out ResearchLevelInfo[] addedResearchLevelInfos,
			out int researchDays) {
			
			int totalExp = 0;
			foreach (var resource in researchedResources) {
				totalExp += resource.GetExpProperty().RealValue.Value;
			}

			return GetLevelIfExpAdded(category, totalExp, out addedResearchLevelInfos, out researchDays);
		}

		public int GetLevelIfExpAdded(ResourceCategory category, int totalExp, out ResearchLevelInfo[] addedResearchLevelInfos,
			out int researchDays) {
			int level = resourceResearchGroups[category].GetLevelIfExpAdded(totalExp, out addedResearchLevelInfos);
			researchDays = Mathf.CeilToInt(addedResearchLevelInfos.Length / (float) expResearchPerDay);
			return level;
		}

		public int AddExp(ResourceCategory category, int exp, out ResearchLevelInfo[] addedResearchLevelInfos) {
			return resourceResearchGroups[category].AddExp(exp, out addedResearchLevelInfos);
		}
	}
}