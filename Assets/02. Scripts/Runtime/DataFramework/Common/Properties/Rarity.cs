namespace Runtime.DataFramework.Properties {
	
	public interface IRarityProperty : IProperty<int>, ILoadFromConfigProperty {
	
	}
	
	public class Rarity : IndependentLoadFromConfigProperty<int>, IRarityProperty {


		protected override PropertyName GetPropertyName() {
			
			return PropertyName.rarity;
		}
	}
}