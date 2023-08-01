using _02._Scripts.Runtime.Common.Properties.SkillsBase;

namespace _02._Scripts.Runtime.Common.Properties {
	public interface IDamageDataProperty: ICustomDataProperty {
		
	}
	
	public class DamageDataProperty: CustomDataProperty<int>, IDamageDataProperty {
		public DamageDataProperty(string name = "damage", IPropertyDependencyModifier<int> modifier = null, params PropertyNameInfo[] dependencies) : base(name, modifier, dependencies) {
			
		}
	}
	
}