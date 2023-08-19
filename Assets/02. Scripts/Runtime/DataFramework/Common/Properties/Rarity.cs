using Runtime.DataFramework.Properties;

namespace Runtime.Enemies.Model.Properties {
	
	public interface IRarityProperty : IProperty<int>, ILoadFromConfigProperty {
	
	}
	
	public class Rarity : IndependentLoadFromConfigProperty<int>, IRarityProperty {


		protected override PropertyName GetPropertyName() {
			
			return PropertyName.rarity;
		}
	}
}