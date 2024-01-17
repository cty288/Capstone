using System;
using System.Collections.Generic;

namespace _02._Scripts.Runtime.ResourceCrafting.Models {
	[Serializable]
	public class ResourceResearchGroup {
		[ES3Serializable]
		private ResearchLevelInfo[] researchLevelInfos;
		[ES3Serializable]
		private int currentLevel;
		[ES3Serializable]
		private int currentExp;
		
		public ResourceResearchGroup(ResearchLevelInfo[] researchLevelInfos) {
			this.currentExp = 0;
			this.currentLevel = 0;
		}

		public ResourceResearchGroup() {
			
		}
		
		public int CurrentLevel => currentLevel;
		
		public int GetLevelIfExpAdded(int exp, out ResearchLevelInfo[] addedResearchLevelInfos) {
			//need to consider current exp
			
			int totalExp = currentExp + exp;
			int level = currentLevel;
			int levelStart = currentLevel;
			while (level < researchLevelInfos.Length && totalExp >= researchLevelInfos[level].TotalExpRequired) {
				level++;
			}

			addedResearchLevelInfos = new ResearchLevelInfo[level - levelStart];
			Array.Copy(this.researchLevelInfos, levelStart, researchLevelInfos, 0, level - levelStart);
			return level;
		}
		
		public int AddExp(int exp, out ResearchLevelInfo[] addedResearchLevelInfos) {
			int level = GetLevelIfExpAdded(exp, out addedResearchLevelInfos);
			currentExp += exp;
			currentLevel = level;
			return level;
		}
		
		
	}
}