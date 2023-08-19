﻿using Runtime.Enemies.Model.Properties;

namespace Runtime.DataFramework.Properties.TestOnly {
	public class TestDanger: AbstractLoadFromConfigProperty<int>, IDangerProperty {
		

		protected override IPropertyDependencyModifier<int> GetDefautModifier() {
			return new TestDangerDefaultModifier();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.danger;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity)};
		}
		
		
	}
	
	public class TestDangerDefaultModifier : PropertyDependencyModifier<int> {
		public override int OnModify(int propertyValue) {
			return propertyValue * GetDependency<Rarity>(new PropertyNameInfo(PropertyName.rarity)).InitialValue * 5;
		}
	}
}