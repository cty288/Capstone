using System.Collections.Generic;
using Framework;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;

namespace _02._Scripts.Runtime.ResourceCrafting.Models.Build {
	public interface IResourceBuildModel : ISavableModel {
		void UnlockBuild(ResourceCategory category, string entityName);
		
		void RemoveFromBuild(ResourceCategory category, string entityName);
	}
	public class ResourceBuildModel : AbstractSavableModel, IResourceBuildModel {
		[field: ES3Serializable] 
		private Dictionary<ResourceCategory, HashSet<string>> buildableResources =
			new Dictionary<ResourceCategory, HashSet<string>>();

		
		public void UnlockBuild(ResourceCategory category, string entityName) {
			if (buildableResources.TryGetValue(category, out var resources)) {
				resources.Add(entityName);
			}
			else {
				buildableResources.Add(category, new HashSet<string>() {entityName});
			}
		}

		public void RemoveFromBuild(ResourceCategory category, string entityName) {
			if (buildableResources.TryGetValue(category, out var resources)) {
				resources.Remove(entityName);
			}
		}
	}
}