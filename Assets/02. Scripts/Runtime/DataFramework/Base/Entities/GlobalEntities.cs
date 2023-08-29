using System;
using System.Collections.Generic;

namespace Runtime.DataFramework.Entities {
	public static class GlobalEntities {
		private static Dictionary<string, (IEntity entity, IEntityModel modelType)> globalEntities = new Dictionary<string, (IEntity, IEntityModel)>();

		public static void RegisterEntity(IEntity entity, IEntityModel model) {
			globalEntities[entity.UUID] = (entity, model);
		}

		public static void UnregisterEntity(string uuid) {
			globalEntities.Remove(uuid);
		}

		public static (IEntity, IEntityModel) GetEntityAndModel(string uuid) {
			if (globalEntities.TryGetValue(uuid, out var tuple)) {
				return tuple;
			}

			
			return (null, null);
		}
		
		public static void Reset() {
			globalEntities.Clear();
		}
	}
}