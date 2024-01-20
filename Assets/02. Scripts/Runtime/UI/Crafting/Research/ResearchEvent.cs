using _02._Scripts.Runtime.ResourceCrafting.Models;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.GameResources.Model.Base;

namespace Runtime.UI.Crafting.Research {
	
	public struct OnResearchEventTriggered {
		public string EventID;
		public int Exp;
		public ResourceCategory Category;
		public ResearchLevelInfo[] ResearchResults;
	}
	
	
	public class ResearchEvent : GameEvent<ResearchEvent> {
		public override EventElapseType ElapseType { get; } = EventElapseType.ExcludeTimeLeap;
		[ES3Serializable]
		private int exp;
		[ES3Serializable]
		private ResourceCategory category;

		public ResourceCategory Category => category;
		
		[ES3Serializable]
		private ResearchLevelInfo[] researchResults;
		public override void OnInitialized() {
			
		}

		public override void OnTriggered() {
			this.SendEvent<OnResearchEventTriggered>(new OnResearchEventTriggered() {
				EventID = this.EventID,
				Exp = exp,
				Category = category,
				ResearchResults = researchResults
			});
			
			
		}

		public override void OnLeaped() {
			
		}

		public override bool CanPersistToOtherLevels { get; } = true;
		public override void OnEventRecycled() {
			
		}
		
		public static ResearchEvent Allocate(int exp, ResourceCategory category, ResearchLevelInfo[] researchResults) {
			ResearchEvent researchEvent = ResearchEvent.Allocate();
			researchEvent.exp = exp;
			researchEvent.category = category;
			researchEvent.researchResults = researchResults;
			return researchEvent;
			
		}
	}
}