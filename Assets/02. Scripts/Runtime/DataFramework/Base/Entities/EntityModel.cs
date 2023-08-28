using System.Collections.Generic;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities.Builders;

namespace Runtime.DataFramework.Entities {
	public interface IEntityModel : IModel {
	
		public IEntity GetEntity(string id);
	
		public bool RemoveEntity(string id);
	
		public int EntityCount { get; }
	}

	public interface IEntityModel<T>: IEntityModel where T: IEntity{
		IEntity IEntityModel.GetEntity(string id) {
			return GetEntity(id);
		}

		public new T GetEntity(string id);
	}

	public interface ICommonEntityModel : IEntityModel<IEntity> {
	
		public T GetEntity<T>(string id) where T : class, IEntity;
	
		/// <summary>
		/// Get the general builder for the entity type
		/// </summary>
		/// <param name="rarity"></param>
		/// <param name="addToModelOnceBuilt">Once the entity is built, add it to the model</param>
		/// <typeparam name="TBuilder">The type of the builder you want to use
		/// You need to make sure that builder is okay to build the target entity</typeparam>
		/// <typeparam name="TEntity"></typeparam>
		/// <returns></returns>
		public TBuilder GetBuilder<TBuilder, TEntity>(int rarity, bool addToModelOnceBuilt = true)
			where TBuilder : EntityBuilder<TBuilder, TEntity>
			where TEntity : class, IEntity, new();
	
		/// <summary>
		/// Get the BasicEntityBuilder for the entity type
		/// </summary>
		/// <param name="rarity"></param>
		/// <param name="addToModelOnceBuilt"></param>
		/// <typeparam name="TEntity"></typeparam>
		/// <returns></returns>
		public BasicEntityBuilder<TEntity> GetBuilder<TEntity>(int rarity, bool addToModelOnceBuilt = true)
			where TEntity : class, IEntity, new();
	}
	public abstract class EntityModel<T> : AbstractSavableModel, IEntityModel<T> where T : IEntity {
		protected IEntityBuilderFactory entityBuilderFactory;
		[field: ES3Serializable]
		protected Dictionary<string, T> entities = new Dictionary<string, T>();

		
		

		protected override void OnInit() {
			base.OnInit();
			entityBuilderFactory = new EntityBuilderFactory();
			foreach (T entity in entities.Values) {
				entity.OnLoadFromSave();
				GlobalEntities.RegisterEntity(entity, this);
				//entity.OnStart();
			}
		}
		
		
	
		protected virtual void OnEntityBuilt(T entity){
			entities.Add(entity.UUID, entity);
			GlobalEntities.RegisterEntity(entity, this);
			//entity.OnStart();
		}

		public T GetEntity(string id){
			if (entities.TryGetValue(id, out var entity)) {
				return entity;
			}
			return default;
		}

		public virtual bool RemoveEntity(string id) {
			if (entities.ContainsKey(id)) {
				IEntity entity = entities[id];
				entity.RecycleToCache();
				entities.Remove(id);
				GlobalEntities.UnregisterEntity(id);
				return true;
			}
			return false;
		}

		public int EntityCount => entities.Count;
	}


	public class CommonEntityModel : EntityModel<IEntity>, ICommonEntityModel {
		public T GetEntity<T>(string id) where T : class, IEntity {
			if (entities.TryGetValue(id, out var entity)) {
				return entity as T;
			}
			return default;
		}

		public TBuilder GetBuilder<TBuilder, TEntity>(int rarity, bool addToModelOnceBuilt = true) where TBuilder : EntityBuilder<TBuilder, TEntity> 
			where TEntity : class, IEntity, new() {
			TBuilder builder = entityBuilderFactory.GetBuilder<TBuilder, TEntity>(rarity);
			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}

			return builder;
		}
	

		public BasicEntityBuilder<TEntity> GetBuilder<TEntity>(int rarity, bool addToModelOnceBuilt = true) where TEntity : class, IEntity, new() {
			BasicEntityBuilder<TEntity> builder = entityBuilderFactory.GetBuilder<BasicEntityBuilder<TEntity>, TEntity>(rarity);
		
			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}

			return builder;
		}
	}
}