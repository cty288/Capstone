using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Commands;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Models.Properties;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using MikroFramework.UIKit;
using UnityEngine;

namespace Runtime.Spawning.Commands {
	public struct OnOpenPillarUI : UIMsg{
		public IPillarEntity pillar;
		public Dictionary<CurrencyType, RewardCostInfo> rewardCosts;
		public Transform rewardSpawnPos;
	}
	public class OpenPillarUICommand : AbstractCommand<OpenPillarUICommand> {
		private PillarEntity pillar;
		private Dictionary<CurrencyType, RewardCostInfo> rewardCosts;
		private CurrencyType pillarCurrencyType;
		private Transform rewardSpawnPos;
		protected override void OnExecute() {
			this.SendEvent<OnOpenPillarUI>(new OnOpenPillarUI() {
				pillar = pillar,
				rewardCosts = rewardCosts,
				rewardSpawnPos = rewardSpawnPos
			});
		}
		
		
		public OpenPillarUICommand() {
			
		}
		
		public static OpenPillarUICommand Allocate(PillarEntity pillar, Dictionary<CurrencyType, RewardCostInfo> rewardCosts,
			Transform rewardSpawnPos) {
			OpenPillarUICommand command = SafeObjectPool<OpenPillarUICommand>.Singleton.Allocate();
			command.pillar = pillar;
			command.rewardSpawnPos = rewardSpawnPos;
			command.rewardCosts = rewardCosts;
			return command;
		}
	}
}