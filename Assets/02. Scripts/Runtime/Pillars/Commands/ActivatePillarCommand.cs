using System;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Commands;
using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.Spawning;

namespace _02._Scripts.Runtime.Pillars.Commands {
	public struct OnRequestActivatePillar{
		public CurrencyType pillarCurrencyType;
		public float CurrencyAmount;
		public int level;
		public IPillarEntity pillarEntity;
	}
	public class ActivatePillarCommand : AbstractCommand<ActivatePillarCommand> {
	
		private CurrencyType pillarCurrencyType;
		private float currencyAmount;
		private int level;
		private IPillarEntity pillarEntity;
		
		protected override void OnExecute() {
			this.SendEvent<OnRequestActivatePillar>(new OnRequestActivatePillar() {
				pillarCurrencyType = pillarCurrencyType,
				CurrencyAmount = currencyAmount,
				level = level,
				pillarEntity = pillarEntity
			});
		}
		
		
		public ActivatePillarCommand() {
			
		}
		
		public static ActivatePillarCommand Allocate(IPillarEntity pillarEntity, CurrencyType pillarCurrencyType, float currencyAmount, int level) {
			ActivatePillarCommand command = SafeObjectPool<ActivatePillarCommand>.Singleton.Allocate();
			command.pillarCurrencyType = pillarCurrencyType;
			command.currencyAmount = currencyAmount;
			command.level = level;
			command.pillarEntity = pillarEntity;
			return command;
		}
	}
}