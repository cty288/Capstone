using _02._Scripts.Runtime.ResourceCrafting.Models;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Polyglot;
using Runtime.GameResources.Model.Base;

namespace Runtime.UI.Crafting.Research {
	
	public struct OnResearchEventTriggered {
		public string EventID;
		public int Exp;
		public ResearchCategory Category;
		public ResearchLevelInfo[] ResearchResults;
		public string NPCName;
	}
	
	
	public class ResearchEvent : GameEvent<ResearchEvent> {
		public override EventElapseType ElapseType { get; } = EventElapseType.ExcludeTimeLeap;
		[ES3Serializable]
		private int exp;
		[ES3Serializable]
		private ResearchCategory category;

		public ResearchCategory Category => category;
		
		[ES3Serializable]
		private ResearchLevelInfo[] researchResults;

		public ResearchLevelInfo[] ResearchResults => researchResults;
		public override void OnInitialized() {
			
		}

		public override void OnTriggered() {
			this.SendEvent<OnResearchEventTriggered>(new OnResearchEventTriggered() {
				EventID = this.EventID,
				Exp = exp,
				Category = category,
				ResearchResults = researchResults,
				NPCName = category == ResearchCategory.Skill ? Localization.Get("NPC_SKILL") : Localization.Get("NPC_GUNSMITH")
			});
			
			
		}

		public override void OnLeaped() {
			
		}

		public override bool CanPersistToOtherLevels { get; } = true;
		public override void OnEventRecycled() {
			
		}
		
		public static ResearchEvent Allocate(int exp, ResearchCategory category, ResearchLevelInfo[] researchResults) {
			ResearchEvent researchEvent = ResearchEvent.Allocate();
			
			
			
			
			researchEvent.exp = exp;
			researchEvent.category = category;
			researchEvent.researchResults = researchResults;
			return researchEvent;
			
		}
	}
}