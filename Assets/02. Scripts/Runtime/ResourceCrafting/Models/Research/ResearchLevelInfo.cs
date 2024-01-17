using System;

namespace _02._Scripts.Runtime.ResourceCrafting.Models {
	
	[Serializable]
	public struct ResearchLevelInfo {
		public int TotalExpRequired;
		public string ResearchedEntityName;
		
		public ResearchLevelInfo(int totalExpRequired, string researchedEntityName) {
			TotalExpRequired = totalExpRequired;
			ResearchedEntityName = researchedEntityName;
		}
	}
}