using System;

namespace _02._Scripts.Runtime.ResourceCrafting.Models {
	
	[Serializable]
	public struct ResearchLevelInfo {
		public int TotalExpRequired;
		public string[] ResearchedEntityNames;
		
		public ResearchLevelInfo(int totalExpRequired, string[] researchedEntityNames) {
			TotalExpRequired = totalExpRequired;
			ResearchedEntityNames = researchedEntityNames;
		}
	}
}