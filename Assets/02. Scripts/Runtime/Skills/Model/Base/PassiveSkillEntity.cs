using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;

namespace _02._Scripts.Runtime.Skills.Model.Base {
	public interface IPassiveSkillEntity: ISkillEntity {
	}
	public abstract class PassiveSkillEntity<T>:  SkillEntity<T>, ISkillEntity, IPassiveSkillEntity  
		where T : SkillEntity<T>, new() {
		
		public override bool HasCooldown() {
			return false;
		}

		public override string InHandVCPrefabName { get; } = null;

		protected override bool GetInventorySwitchCondition(Dictionary<CurrencyType, int> currency) {
			return false;
		}
		
	}
}