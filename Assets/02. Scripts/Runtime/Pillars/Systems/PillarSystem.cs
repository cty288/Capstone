using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Pillars.Commands;
using _02._Scripts.Runtime.Pillars.Models;
using MikroFramework.Architecture;
using Runtime.Spawning;

namespace _02._Scripts.Runtime.Pillars.Systems {
	public interface IPillarSystem : ISystem {
		
	}
	public class PillarSystem : AbstractSystem, IPillarSystem {
		private Dictionary<CurrencyType, IPillarEntity> 
			currentLevelPillars = new Dictionary<CurrencyType, IPillarEntity>();
		
		protected override void OnInit() {
			this.RegisterEvent<OnSetCurrentLevelPillars>(OnSetCurrentLevelPillars);
			this.RegisterEvent<OnRequestActivatePillar>(OnRequestActivatePillar);
		}

		private void OnRequestActivatePillar(OnRequestActivatePillar e) {
			if (currentLevelPillars.ContainsKey(e.pillarCurrencyType)) {
				IPillarEntity pillarEntity = currentLevelPillars[e.pillarCurrencyType];
				if (pillarEntity.Status.Value == PillarStatus.Idle) {
					ActivatePillar(pillarEntity);
				}
			}
		}
		
		private void ActivatePillar(IPillarEntity pillarEntity) {
			pillarEntity.Status.Value = PillarStatus.Activated;
			
			//check if all pillars are activated
			bool allPillarsActivated = true;
			foreach (var pillar in currentLevelPillars) {
				if (pillar.Value.Status.Value != PillarStatus.Activated) {
					allPillarsActivated = false;
					break;
				}
			}

			if (allPillarsActivated) {
				
			}
		}

		private void OnSetCurrentLevelPillars(OnSetCurrentLevelPillars e) {
			//new level, reset
			currentLevelPillars.Clear();
			foreach (IPillarEntity pillarEntity in e.pillars) {
				currentLevelPillars.Add(pillarEntity.PillarCurrencyType, pillarEntity);
			}
		}
	}
}