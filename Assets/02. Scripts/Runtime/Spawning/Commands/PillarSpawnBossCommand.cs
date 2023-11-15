using System.Collections.Generic;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models.Properties;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using UnityEngine;

namespace Runtime.Spawning.Commands {
	public struct OnRequestPillarSpawnBoss {
		public GameObject Pillar;
		public int Rarity;
	}
	public class PillarSpawnBossCommand : AbstractCommand<PillarSpawnBossCommand> {
		private GameObject pillar;
		private Dictionary<CurrencyType, int> totalCosts;
		private int rarity;
		protected override void OnExecute() {
			ICurrencySystem currencySystem = this.GetSystem<ICurrencySystem>();
			foreach (CurrencyType currencyType in totalCosts.Keys) {
				currencySystem.RemoveCurrency(currencyType, totalCosts[currencyType]);
			}
			this.SendEvent<OnRequestPillarSpawnBoss>(new OnRequestPillarSpawnBoss() {
				Pillar = pillar,
				Rarity = rarity
			});
		}
		
		
		public PillarSpawnBossCommand() {
			
		}
		
		public static PillarSpawnBossCommand Allocate(GameObject pillar, Dictionary<CurrencyType, int> totalCosts, int rarity) {
			PillarSpawnBossCommand command = SafeObjectPool<PillarSpawnBossCommand>.Singleton.Allocate();
			command.pillar = pillar;
			command.totalCosts = totalCosts;
			command.rarity = rarity;
			return command;
		}
	}
}