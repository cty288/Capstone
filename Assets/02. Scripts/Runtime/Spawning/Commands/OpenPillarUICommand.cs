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
		public Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts;
	}
	public class OpenPillarUICommand : AbstractCommand<OpenPillarUICommand> {
		private GameObject pillar;
		private Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts;
		protected override void OnExecute() {
			this.SendEvent<OnOpenPillarUI>(new OnOpenPillarUI() {
				pillar = pillar,
				bossSpawnCosts = bossSpawnCosts
			});
		}
		
		
		public OpenPillarUICommand() {
			
		}
		
		public static OpenPillarUICommand Allocate(GameObject pillar, Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts) {
			OpenPillarUICommand command = SafeObjectPool<OpenPillarUICommand>.Singleton.Allocate();
			command.pillar = pillar;
			command.bossSpawnCosts = bossSpawnCosts;
			return command;
		}
	}
}