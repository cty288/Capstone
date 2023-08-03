
/*
using _02._Scripts.Runtime.Common.Properties.SkillsBase;

namespace _02._Scripts.Runtime.Common.Properties{
	public interface ISkillProperty : ICustomProperty {
		public string GetSkillName();
		
		string ICustomProperty.GetCustomPropertyName() {
			return GetSkillName();
		}

	}


	public class AbstractSkillProperty : AbstractCustomProperty, ISkillProperty {

		public AbstractSkillProperty() : base() {
			
		}

		public string GetSkillName() {
			throw new System.NotImplementedException();
		}


		public override string GetCustomPropertyName() {
			return GetSkillName();
		}

		public override string OnGetDescription() {
			throw new System.NotImplementedException();
		}

		public override ICustomDataProperty[] GetCustomDataProperties() {
			throw new System.NotImplementedException();
		}
	}
}*/