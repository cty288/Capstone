namespace Runtime.DataFramework.Properties {
	public interface IRarityProperty : IProperty<int>, ILoadFromConfigProperty {
	
	}
	
	public class Rarity : IndependentLoadFromConfigProperty<int>, IRarityProperty {

		public override int OnSetBaseValueFromConfig(dynamic value) {
			return value;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.rarity;
		}
	}
}