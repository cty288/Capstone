using System.Collections.Generic;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.ResourceCrafting.Models;
using _02._Scripts.Runtime.Skills.Commands;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.RawMaterials.Model.Base;
using Runtime.UI.Crafting.Research;

namespace _02._Scripts.Runtime.ResourceCrafting.Commands.Research {
	public struct OnStartResearch {
		public string EventID;
	}
	public class ResearchCommand : AbstractCommand<ResearchCommand> {
		private int money;
		private int exp;
		private int days;
		private ResourceCategory category;
		private HashSet<ResourceSlot> selectedResources;
		
		private ICurrencySystem currencySystem;
		private IGameEventSystem gameEventSystem;
		private ICurrencyModel currencyModel;
		private ResearchLevelInfo[] researchResults;
		private IResearchSystem researchSystem;
		
		public ResearchCommand() {
			
		}

		public static ResearchCommand Allocate(int money, int exp, int days,
			ResourceCategory category, HashSet<ResourceSlot> selectedResources,
			ResearchLevelInfo[] researchResults) {
			
			ResearchCommand command = SafeObjectPool<ResearchCommand>.Singleton.Allocate();
			command.money = money;
			command.exp = exp;
			command.days = days;
			command.selectedResources = selectedResources;
			command.category = category;
			command.researchResults = researchResults;
			
			
			
			return command;
		}

		protected override void OnExecute() {
			currencySystem = this.GetSystem<ICurrencySystem>();
			gameEventSystem = this.GetSystem<IGameEventSystem>();
			currencyModel = this.GetModel<ICurrencyModel>();
			researchSystem = this.GetSystem<IResearchSystem>();
			
			
			currencySystem.RemoveMoney(money);
			foreach (var resource in selectedResources) {
				foreach (string id in resource.GetUUIDList()) {
					GlobalEntities.GetEntityAndModel(id).Item2.RemoveEntity(id);
				}
				resource.Clear();
			}

			if (!researchSystem.IsResearching(category)) {
				string eventID = gameEventSystem.AddEvent(ResearchEvent.Allocate(exp, category, researchResults),
					days * 24 * 60);
				
				this.SendEvent<OnStartResearch>(new OnStartResearch() {
					EventID = eventID
				});
			}
			
			
		}
	}
}