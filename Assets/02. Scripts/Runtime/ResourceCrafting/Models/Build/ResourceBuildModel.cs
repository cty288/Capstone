using System.Collections.Generic;
using Framework;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;

namespace _02._Scripts.Runtime.ResourceCrafting.Models.Build {
	public interface IResourceBuildModel : ISavableModel {
		void UnlockBuild(ResearchCategory category, string entityName, bool isNew);
		
		void RemoveFromBuild(ResearchCategory category, string entityName);
		
		HashSet<string> GetBuildableResources(ResearchCategory category);
		
		void SetIsNew(string entityName, bool isNew);
		
		bool IsNew(string entityName);
	}
	public class ResourceBuildModel : AbstractSavableModel, IResourceBuildModel {
		[field: ES3Serializable] 
		private Dictionary<ResearchCategory, HashSet<string>> buildableResources =
			new Dictionary<ResearchCategory, HashSet<string>>();

		[field: ES3Serializable]
		private Dictionary<string, bool> isNewBuildableResources = new Dictionary<string, bool>();

		protected override void OnInit() {
			base.OnInit();
			//UnlockBuild(ResearchCategory.Skill, "TurretSkill", true);
		}

		public void UnlockBuild(ResearchCategory category, string entityName, bool isNew) {
			if (buildableResources.TryGetValue(category, out var resources)) {
				resources.Add(entityName);
			}else {
				buildableResources.Add(category, new HashSet<string>() {entityName});
			}
			
			if (isNewBuildableResources.TryGetValue(entityName, out var isnew)) {
				isNewBuildableResources[entityName] = isNew;
			}
			else {
				isNewBuildableResources.Add(entityName, isNew);
			}
		}

		public void RemoveFromBuild(ResearchCategory category, string entityName) {
			if (buildableResources.TryGetValue(category, out var resources)) {
				resources.Remove(entityName);
			}
		}

		public HashSet<string> GetBuildableResources(ResearchCategory category) {
			if (buildableResources.TryGetValue(category, out var resources)) {
				return resources;
			}
			else {
				return new HashSet<string>();
			}
		}

		public void SetIsNew(string entityName, bool isNew) {
			if (isNewBuildableResources.TryGetValue(entityName, out _)) {
				isNewBuildableResources[entityName] = isNew;
			}
			else {
				isNewBuildableResources.Add(entityName, isNew);
			}
		}

		public bool IsNew(string entityName) {
			if (isNewBuildableResources.TryGetValue(entityName, out var isNew)) {
				return isNew;
			}
			else {
				return false;
			}
		}
	}
}