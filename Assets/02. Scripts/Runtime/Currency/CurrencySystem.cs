using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;

namespace _02._Scripts.Runtime.Currency {
	public interface ICurrencySystem : ISystem {
		
	}
	public class CurrencySystem : AbstractSystem, ICurrencySystem {
		private ICurrencyModel currencyModel;
		protected override void OnInit() {
			this.RegisterEvent<OnSkillUsed>(OnSkillUsed);
			currencyModel = this.GetModel<ICurrencyModel>();
		}

		private void OnSkillUsed(OnSkillUsed e) {
			Dictionary<CurrencyType, int> costs = e.skillEntity.GetSkillUseCostOfCurrentLevel();
			foreach (KeyValuePair<CurrencyType, int> cost in costs) {
				currencyModel.RemoveCurrency(cost.Key, cost.Value);
			}
		}
	}
}