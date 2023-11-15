using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Builder;

namespace Runtime.GameResources.Model.Base {
	
	public interface IGameResourceModel<T> : IModel, IEntityModel<T>, ISavableModel where T : IResourceEntity {
		/// <summary>
		/// Get any resource, as long as it inherits from IResourceEntity
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public IResourceEntity GetAnyResource(string id);
	}
	
	public static class GlobalGameResourceEntities {
		public static Dictionary<string, IResourceEntity> globalResourceOfSameType = new Dictionary<string, IResourceEntity>();
		
		public static IResourceEntity GetAnyResource(string id) {
			if (id == null) {
				return null;
			}
			if (globalResourceOfSameType.TryGetValue(id, out var resource)) {
				return resource;
			}
			return null;
		}
		
		public static void Reset() {
			globalResourceOfSameType.Clear();
		}
	}
	
	public abstract class GameResourceModel<T> : EntityModel<T>, IGameResourceModel<T>
	 where T : IResourceEntity {
		
		protected override void OnInit() {
			base.OnInit();
			//GlobalGameResourceEntities.globalResourceOfSameType.Clear();
			foreach (T entity in entities.Values) {
				GlobalGameResourceEntities.globalResourceOfSameType.Add(entity.UUID, entity);
			}
		}

		protected override void OnEntityBuilt(T entity) {
			base.OnEntityBuilt(entity);
			GlobalGameResourceEntities.globalResourceOfSameType.Add(entity.UUID, entity);
		}

		public IResourceEntity GetAnyResource(string id) {
			return GlobalGameResourceEntities.GetAnyResource(id);
		}

		public override bool RemoveEntity(string id, bool force = false) {
			bool success = base.RemoveEntity(id, force);
			if (success) {
				GlobalGameResourceEntities.globalResourceOfSameType.Remove(id);
			}
			return success;
		}
	}
}