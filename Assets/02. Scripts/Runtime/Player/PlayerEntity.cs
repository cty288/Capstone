using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.Player {
	public class PlayerEntity : AbstractCreature {
		public override string EntityName { get; set; } = "Player";
		protected override ConfigTable GetConfigTable() {
			throw new System.NotImplementedException();
		}

		public override void OnDoRecycle() {
			throw new System.NotImplementedException();
		}

		public override void OnRecycle() {
			throw new System.NotImplementedException();
		}

		protected override string OnGetDescription(string defaultLocalizationKey) {
			throw new System.NotImplementedException();
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			throw new System.NotImplementedException();
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			throw new System.NotImplementedException();
		}

		protected override Faction GetDefaultFaction() {
			throw new System.NotImplementedException();
		}
	}
}