using _02._Scripts.Runtime.ResourceCrafting.Commands.Research;
using _02._Scripts.Runtime.ResourceCrafting.Models;
using _02._Scripts.Runtime.ResourceCrafting.Models.Build;
using MikroFramework.Architecture;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;

namespace Runtime.UI.Crafting.Research {
	public interface IResearchSystem : ISystem {
		bool IsResearching(ResourceCategory category);
		
		ResearchEvent GetResearchEvent(ResourceCategory category);
		
	}

	public class ResearchSystem : AbstractSystem, IResearchSystem {
		private IGameEventModel gameEventModel;
		private IResourceResearchModel resourceResearchModel;
		private IResourceBuildModel resourceBuildModel;
		
		protected override void OnInit() {
			gameEventModel = this.GetModel<IGameEventModel>();
			resourceResearchModel = this.GetModel<IResourceResearchModel>();
			resourceBuildModel = this.GetModel<IResourceBuildModel>();
			this.RegisterEvent<OnStartResearch>(OnStartResearch);
			this.RegisterEvent<OnResearchEventTriggered>(OnResearchEventTriggered);

		}

		private void OnResearchEventTriggered(OnResearchEventTriggered e) {
			if(e.EventID == GetResearchEvent(e.Category)?.EventID) {
				resourceResearchModel.RemoveResearchEvent(e.Category);

				resourceResearchModel.AddExp(e.Category, e.Exp, out ResearchLevelInfo[] addedResearchLevelInfos);
				foreach (ResearchLevelInfo researchLevelInfo in addedResearchLevelInfos) {
					if (researchLevelInfo.ResearchedEntityNames == null) {
						continue;
					}
					foreach (string entityName in researchLevelInfo.ResearchedEntityNames) {
						resourceBuildModel.UnlockBuild(e.Category, entityName);
					}
				}
			}
		}

		private void OnStartResearch(OnStartResearch e) {
			string id = e.EventID;
			ResearchEvent researchEvent = gameEventModel.GetEvent(id) as ResearchEvent;
			resourceResearchModel.OnResearchEvent(researchEvent);
		}


		public bool IsResearching(ResourceCategory category) {
			return GetResearchEvent(category) != null;
		}

		public ResearchEvent GetResearchEvent(ResourceCategory category) {
			string currentResearchEventID = resourceResearchModel.GetCurrentResearchEventID(category);
			
			if (string.IsNullOrEmpty(currentResearchEventID)) {
				return null;
			}
			
			ResearchEvent researchEvent = gameEventModel.GetEvent(currentResearchEventID) as ResearchEvent;
			if(researchEvent == null) {
				return null;
			}

			if (researchEvent.IsRecycled) {
				return null;
			}
			else {
				return researchEvent;
			}
		}
	}
}