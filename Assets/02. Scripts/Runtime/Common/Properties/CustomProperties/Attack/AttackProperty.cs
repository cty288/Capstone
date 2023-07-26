using _02._Scripts.Runtime.Common.Properties.SkillsBase;

namespace _02._Scripts.Runtime.Common.Properties {
	public interface IAttackproperty : ICustomProperty {
		public ICustomDataProperty GetDamageDataProperty();
	}
	
	public class AttackProperty : AbstractCustomProperty, IAttackproperty {
		public override string GetCustomPropertyName() {
			throw new System.NotImplementedException();
		}

		public override string OnGetDescription() {
			throw new System.NotImplementedException();
		}

		public ICustomDataProperty GetDamageDataProperty() {
			throw new System.NotImplementedException();
		}

		public override ICustomDataProperty[] GetCustomDataProperties() {
			throw new System.NotImplementedException();
		}
	}
}