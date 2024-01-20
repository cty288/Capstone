﻿using System;
using System.Collections.Generic;
using Framework;
using Runtime.GameResources.Model.Base;
using Runtime.RawMaterials.Model.Base;
using Runtime.UI.Crafting.Research;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

namespace _02._Scripts.Runtime.ResourceCrafting.Models {
	public interface IResourceResearchModel : ISavableModel {
		
		public int GetLevelIfExpAdded(ResourceCategory category,
			IHaveExpResourceEntity[] researchedResources, out ResearchLevelInfo[] addedResearchLevelInfos, out int researchDays);
		
		public int GetLevelIfExpAdded(ResourceCategory category,
			int totalExp, out ResearchLevelInfo[] addedResearchLevelInfos, out int researchDays);

		public int AddExp(ResourceCategory category, int exp, out ResearchLevelInfo[] addedResearchLevelInfos);
		
		public bool IsAllResearched(ResourceCategory category);

		public void OnResearchEvent(ResearchEvent researchEvent);

		public string GetCurrentResearchEventID(ResourceCategory category);
		
		public void RemoveResearchEvent(ResourceCategory category);

	}
	public class ResourceResearchModel : AbstractSavableModel, IResourceResearchModel {
		public static int ExpResearchPerDay = 7;
		public static float CostPerExp = 10f;

		[ES3Serializable] 
		private Dictionary<ResourceCategory, ResourceResearchGroup> resourceResearchGroups =
			new Dictionary<ResourceCategory, ResourceResearchGroup>();

		[ES3Serializable]
		private Dictionary<ResourceCategory, string> currentResearchEventIDs =
			new Dictionary<ResourceCategory, string>();

		protected override void OnInit() {
			base.OnInit();
			if (IsFirstTimeCreated) {
				ExpResearchPerDay =
					int.Parse(ConfigDatas.Singleton.GlobalDataTable.Get<string>("EXP_RESEARCH_PER_DAY", "Value1"));

				CostPerExp = float.Parse(ConfigDatas.Singleton.GlobalDataTable.Get<string>("COST_PER_EXP", "Value1"));
				
				LoadResourceGroupsFromConfig(ResourceCategory.Skill, ConfigDatas.Singleton.ResearchSkillsConfigTable);
				LoadResourceGroupsFromConfig(ResourceCategory.WeaponParts,
					ConfigDatas.Singleton.ResearchWeaponPartsConfigTable);
			}
		}


		protected void LoadResourceGroupsFromConfig(ResourceCategory category, ConfigTable table) {
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
			researchDays = Mathf.CeilToInt(totalExp / (float) ExpResearchPerDay);
			return level;
		}

		public int AddExp(ResourceCategory category, int exp, out ResearchLevelInfo[] addedResearchLevelInfos) {
			return resourceResearchGroups[category].AddExp(exp, out addedResearchLevelInfos);
		}

		public bool IsAllResearched(ResourceCategory category) {
			return resourceResearchGroups[category].IsMaxLevel();
		}

		public void OnResearchEvent(ResearchEvent researchEvent) {
			currentResearchEventIDs[researchEvent.Category] = researchEvent.EventID;
		}

		public string GetCurrentResearchEventID(ResourceCategory category) {
			if (!currentResearchEventIDs.ContainsKey(category)) {
				return null;
			}
			return currentResearchEventIDs[category];
		}

		public void RemoveResearchEvent(ResourceCategory category) {
			currentResearchEventIDs.Remove(category);
		}
	}
}