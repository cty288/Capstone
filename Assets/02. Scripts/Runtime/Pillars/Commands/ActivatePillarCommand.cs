using System;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Commands;
using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;
using MikroFramework.Pool;

namespace _02._Scripts.Runtime.Pillars.Commands {
	public struct OnRequestActivatePillar{
		public CurrencyType pillarCurrencyType;
	}
	public class ActivatePillarCommand : AbstractCommand<ActivatePillarCommand> {
	
		private CurrencyType pillarCurrencyType;
		
		protected override void OnExecute() {
			this.SendEvent<OnRequestActivatePillar>(new OnRequestActivatePillar() {
				pillarCurrencyType = pillarCurrencyType
			});
		}
		
		
		public ActivatePillarCommand() {
			
		}
		
		public static ActivatePillarCommand Allocate(CurrencyType pillarCurrencyType) {
			ActivatePillarCommand command = SafeObjectPool<ActivatePillarCommand>.Singleton.Allocate();
			command.pillarCurrencyType = pillarCurrencyType;
			return command;
		}
	}
}