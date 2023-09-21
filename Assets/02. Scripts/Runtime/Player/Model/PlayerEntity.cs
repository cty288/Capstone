using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.Player {
	public interface IPlayerEntity : ICreature, IEntity {
		
	}
	
	public class PlayerEntity : AbstractCreature, IPlayerEntity {
		public override string EntityName { get; set; } = "Player";
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.PlayerEntityConfigTable;
		}

		public override void OnDoRecycle() {
			SafeObjectPool<PlayerEntity>.Singleton.Recycle(this as PlayerEntity);
		}

		public override void OnRecycle() {
			
		}

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return "";
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}

		protected override Faction GetDefaultFaction() {
			return Faction.Friendly;
		}
	}
}