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
		public GameObject pillar;
		public RewardCostInfo rewardCosts;
		public CurrencyType pillarCurrencyType;
	}
	public class OpenPillarUICommand : AbstractCommand<OpenPillarUICommand> {
		private GameObject pillar;
		private RewardCostInfo rewardCosts;
		private CurrencyType pillarCurrencyType;
		protected override void OnExecute() {
			this.SendEvent<OnOpenPillarUI>(new OnOpenPillarUI() {
				pillar = pillar,
				rewardCosts = rewardCosts,
				pillarCurrencyType = pillarCurrencyType
			});
		}
		
		
		public OpenPillarUICommand() {
			
		}
		
		public static OpenPillarUICommand Allocate(GameObject pillar, RewardCostInfo rewardCosts, CurrencyType pillarCurrencyType) {
			OpenPillarUICommand command = SafeObjectPool<OpenPillarUICommand>.Singleton.Allocate();
			command.pillar = pillar;
			command.rewardCosts = rewardCosts;
			command.pillarCurrencyType = pillarCurrencyType;
			return command;
		}
	}
}