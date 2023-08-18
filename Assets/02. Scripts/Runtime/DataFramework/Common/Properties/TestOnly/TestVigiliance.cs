﻿namespace Runtime.DataFramework.Properties.TestOnly {
	public interface IVigilianceProperty : IProperty<float>, ILoadFromConfigProperty {
		
	}
	public class TestVigiliance: AbstractLoadFromConfigProperty<float>, IVigilianceProperty {
		

		protected override IPropertyDependencyModifier<float> GetDefautModifier() {
			return new VigilianceDefaultModifier();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.vigiliance;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity)};
		}
		
		
	}
	
	public class VigilianceDefaultModifier : PropertyDependencyModifier<float> {
		public override float OnModify(float propertyValue) {
			return propertyValue * GetDependency<Rarity>(new PropertyNameInfo(PropertyName.rarity)).InitialValue * 5;
		}
	}
}