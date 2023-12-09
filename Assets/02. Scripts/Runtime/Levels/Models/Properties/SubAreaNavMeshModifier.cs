using Runtime.DataFramework.Properties;

namespace _02._Scripts.Runtime.Levels.Models.Properties {
	public interface ISubAreaNavMeshModifier : IProperty<int> {
		
	}
	public class SubAreaNavMeshModifier : Property<int>, ISubAreaNavMeshModifier {
		protected override IPropertyDependencyModifier<int> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.sub_area_nav_mesh_modifier;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties()
		{
			return null;
		}
	}
}