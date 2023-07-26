using _02._Scripts.Runtime.Common.Properties.SkillsBase;

namespace _02._Scripts.Runtime.Common.Properties {
	public interface IDamageDataProperty: ICustomDataProperty {
		
	}
	
	public class DamageDataProperty<T>: CustomDataProperty<T>, IDamageDataProperty {
		public DamageDataProperty(string name = "damage", IPropertyDependencyModifier<T> modifier = null, params PropertyNameInfo[] dependencies) : base(name, modifier, dependencies) {
			
		}
	}
	
}