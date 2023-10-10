using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;

namespace _02._Scripts.Runtime.Levels.Models {
	public interface ILevelEntity : IEntity, IHaveCustomProperties, IHaveTags {
		
	}
	
	public abstract class LevelEntity<T> : AbstractBasicEntity, ILevelEntity where T : LevelEntity<T>, new() {
		public override string EntityName { get; set; }
		protected override ConfigTable GetConfigTable() {
			return null;
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		public override void OnDoRecycle() {
			SafeObjectPool<T>.Singleton.Recycle(this as T);
		}

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}

		

		protected override void OnEntityRegisterAdditionalProperties() {
			
		}
		
	}
}