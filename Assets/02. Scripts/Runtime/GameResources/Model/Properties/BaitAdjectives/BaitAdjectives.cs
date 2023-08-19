using System.Collections.Generic;
using System.Text;
using Polyglot;
using Runtime.DataFramework.Description;
using Runtime.DataFramework.Properties;

namespace Runtime.GameResources.Model.Properties.BaitAdjectives {

	public interface IBaitAdjectives : IHashSetProperty<BaitAdjective>, IHaveDescription, ILoadFromConfigProperty {
		
	}
	public class BaitAdjectives: LoadFromConfigHashsetProperty<BaitAdjective>, IBaitAdjectives {
		protected override IPropertyDependencyModifier<HashSet<BaitAdjective>> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.bait_adjectives;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}

		public string GetDescription() {
			if (RealValues.Value != null) {
				//adj format: bait_adj_{name}, separated by comma
				string[] adjNamesLocalized = new string[RealValues.Value.Count];
			
				int i = 0;
				foreach (BaitAdjective adj in RealValues.Value) {
					adjNamesLocalized[i] = Localization.Get($"bait_adj_{adj.ToString()}_desc");
					i++;
				}

				return string.Join(Localization.Get("COMMA"), adjNamesLocalized);
			}

			return "";
		}
	}
}