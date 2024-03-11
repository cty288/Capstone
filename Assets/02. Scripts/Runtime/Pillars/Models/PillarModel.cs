using System.Collections.Generic;
using _02._Scripts.Runtime.Pillars.Systems;
using Framework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.Spawning;

namespace _02._Scripts.Runtime.Pillars.Models {
	public struct OnSetCurrentLevelPillars {
		public List<IPillarEntity> pillars;
		public int levelCount;
	}
	public interface IPillarModel : IEntityModel, ISavableModel {
		PillarBuilder<T> GetPillarBuilder<T>(bool addToModelOnceBuilt = true)
			where T : class, IPillarEntity, new();

		void SetCurrentLevelPillars(HashSet<string> ids, int levelCount);
		List<IPillarEntity> GetCurrentLevelPillars();
		
		Dictionary<string, PillarActivateInfo> ActivatedPillarCurrencyAmount  { get; }
	}
	
	public class PillarModel: EntityModel<IPillarEntity>, IPillarModel {

		private List<IPillarEntity> currentLevelPillars = new List<IPillarEntity>();
		
		[ES3Serializable]
		private Dictionary<string, PillarActivateInfo>
			activatedPillarCurrencyAmount = new Dictionary<string, PillarActivateInfo>();
		public Dictionary<string, PillarActivateInfo> ActivatedPillarCurrencyAmount => activatedPillarCurrencyAmount;
		public PillarBuilder<T> GetPillarBuilder<T>(bool addToModelOnceBuilt = true) where T : class, IPillarEntity, new() {
			PillarBuilder<T> builder = entityBuilderFactory.GetBuilder<PillarBuilder<T>, T>(1);

			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}
			

			return builder;
		}

		public void SetCurrentLevelPillars(HashSet<string> ids, int levelCount) {
			currentLevelPillars.Clear();

			foreach (var id in ids) {
				IPillarEntity entity = GetEntity(id);
				if (entity != null) {
					currentLevelPillars.Add(entity);
				}
			}

			this.SendEvent<OnSetCurrentLevelPillars>(new OnSetCurrentLevelPillars() {
				pillars = GetCurrentLevelPillars(),
				levelCount = levelCount
			});
		}

		public List<IPillarEntity> GetCurrentLevelPillars() {
			return currentLevelPillars;
		}

		
	}
}