using System.Linq;

namespace Runtime.DataFramework.Properties {
	public enum TasteType {
		Type1,
		Type2,
		Type3
	}

	public interface ITasteProperty : IListProperty<TasteType>, ILoadFromConfigProperty {
		
	}
	public class Taste : IndependentLoadFromConfigListProperty<TasteType>, ITasteProperty {

		protected override PropertyName GetPropertyName() {
			return PropertyName.taste;
		}
		
		public Taste(): base(){}
		
		public Taste(params TasteType[] baseValues) : base() {
			BaseValue = baseValues.ToList();
		}
	}
}