using System;
using System.Collections.Generic;
using Framework;
using Runtime.GameResources.Model.Base;
using Runtime.RawMaterials.Model.Base;
using Runtime.UI.Crafting.Research;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

namespace _02._Scripts.Runtime.ResourceCrafting.Models {
	public enum ResearchCategory {
		Skill,
		WeaponAndParts
		
	}
	
	public static class ResearchCategoryExtension {
		public static ResourceCategory[] ToResourceCategory(this ResearchCategory category) {
			switch (category) {
				case ResearchCategory.Skill:
					return new ResourceCategory[] {ResourceCategory.Skill};
				case ResearchCategory.WeaponAndParts:
					return new ResourceCategory[] {ResourceCategory.WeaponParts, ResourceCategory.Weapon};
				default:
					throw new ArgumentOutOfRangeException(nameof(category), category, null);
			}
		}
		
		public static ResearchCategory ToResearchCategory(this ResourceCategory category) {
			switch (category) {
				case ResourceCategory.Skill:
					return ResearchCategory.Skill;
				case ResourceCategory.WeaponParts:
				case ResourceCategory.Weapon:
					return ResearchCategory.WeaponAndParts;
				default:
					throw new ArgumentOutOfRangeException(nameof(category), category, null);
			}
		}
	}
	
	
	public interface IResourceResearchModel : ISavableModel {
		
		public int GetLevelIfExpAdded(ResearchCategory category,
			IHaveExpResourceEntity[] researchedResources, out ResearchLevelInfo[] addedResearchLevelInfos, out int researchDays);
		
		public int GetLevelIfExpAdded(ResearchCategory category,
			int totalExp, out ResearchLevelInfo[] addedResearchLevelInfos, out int researchDays);

		public int AddExp(ResearchCategory category, int exp, out ResearchLevelInfo[] addedResearchLevelInfos);
		
		public bool IsAllResearched(ResearchCategory category);

		public void OnResearchEvent(ResearchEvent researchEvent);

		public string GetCurrentResearchEventID(ResearchCategory category);
		
		public void RemoveResearchEvent(ResearchCategory category);

	}
	public class ResourceResearchModel : AbstractSavableModel, IResourceResearchModel {
		public static int ExpResearchPerDay = 7;
		public static float CostPerExp = 10f;

		[ES3Serializable] 
		private Dictionary<ResearchCategory, ResourceResearchGroup> resourceResearchGroups =
			new Dictionary<ResearchCategory, ResourceResearchGroup>();

		[ES3Serializable]
		private Dictionary<ResearchCategory, string> currentResearchEventIDs =
			new Dictionary<ResearchCategory, string>();

		protected override void OnInit() {
			base.OnInit();
			if (IsFirstTimeCreated) {
				ExpResearchPerDay =
					int.Parse(ConfigDatas.Singleton.GlobalDataTable.Get<string>("EXP_RESEARCH_PER_DAY", "Value1"));

				CostPerExp = float.Parse(ConfigDatas.Singleton.GlobalDataTable.Get<string>("COST_PER_EXP", "Value1"));
				
				LoadResourceGroupsFromConfig(ResearchCategory.Skill, ConfigDatas.Singleton.ResearchSkillsConfigTable);
				LoadResourceGroupsFromConfig(ResearchCategory.WeaponAndParts,
					ConfigDatas.Singleton.ResearchWeaponPartsConfigTable);
			}
		}


		protected void LoadResourceGroupsFromConfig(ResearchCategory category, ConfigTable table) {
			int index = 0;
			List<ResearchLevelInfo> researchLevelInfos = new List<ResearchLevelInfo>();
			
			while (table.HasEntity(index.ToString())) {
				int totalExp = table.Get<int>(index.ToString(), "exp");
				string[] entityName = table.Get<String[]>(index.ToString(), "EntityNames");
				researchLevelInfos.Add(new ResearchLevelInfo(totalExp, entityName));
				index++;
			}

			resourceResearchGroups.Add(category, new ResourceResearchGroup(researchLevelInfos.ToArray()));
		}

		public int GetLevelIfExpAdded(ResearchCategory category,
			IHaveExpResourceEntity[] researchedResources, out ResearchLevelInfo[] addedResearchLevelInfos,
			out int researchDays) {
			
			int totalExp = 0;
			foreach (var resource in researchedResources) {
				totalExp += resource.GetExpProperty().RealValue.Value;
			}

			return GetLevelIfExpAdded(category, totalExp, out addedResearchLevelInfos, out researchDays);
		}

		public int GetLevelIfExpAdded(ResearchCategory category, int totalExp, out ResearchLevelInfo[] addedResearchLevelInfos,
			out int researchDays) {
			int level = resourceResearchGroups[category].GetLevelIfExpAdded(totalExp, out addedResearchLevelInfos);
			researchDays = Mathf.CeilToInt(totalExp / (float) ExpResearchPerDay);
			return level;
		}

		public int AddExp(ResearchCategory category, int exp, out ResearchLevelInfo[] addedResearchLevelInfos) {
			return resourceResearchGroups[category].AddExp(exp, out addedResearchLevelInfos);
		}

		public bool IsAllResearched(ResearchCategory category) {
			return resourceResearchGroups[category].IsMaxLevel();
		}

		public void OnResearchEvent(ResearchEvent researchEvent) {
			currentResearchEventIDs[researchEvent.Category] = researchEvent.EventID;
		}

		public string GetCurrentResearchEventID(ResearchCategory category) {
			if (!currentResearchEventIDs.ContainsKey(category)) {
				return null;
			}
			return currentResearchEventIDs[category];
		}

		public void RemoveResearchEvent(ResearchCategory category) {
			currentResearchEventIDs.Remove(category);
		}
	}
}