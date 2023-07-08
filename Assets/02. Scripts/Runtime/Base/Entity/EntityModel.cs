using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Base.Entity;using MikroFramework.Architecture;
using UnityEngine;


/// <summary>
/// This is the only place where all entities are saved, no reference to any entity is allowed outside this class
/// To reference an entity, use the entity's id
/// </summary>
public interface IEntityModel: IModel {
	/// <summary>
	/// Get the builder for the entity type
	/// </summary>
	/// <param name="addToModelOnceBuilt">Once the entity is built, add it to the model</param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public EntityBuilder<T> GetBuilder<T>(bool addToModelOnceBuilt = true) where T : class, IEntity, new();

	public T GetEntity<T>(string id) where T : class, IEntity, new();
	
	public bool RemoveEntity(string id);
	
	public int EntityCount { get; }
}
public class EntityModel : AbstractSavableModel, IEntityModel {
	private IEntityBuilderFactory entityBuilderFactory;
	[field: ES3Serializable]
	private Dictionary<string, IEntity> entities = new Dictionary<string, IEntity>();

	protected override void OnInit() {
		base.OnInit();
		entityBuilderFactory = new EntityBuilderFactory();
	}

	public EntityBuilder<T> GetBuilder<T>(bool addToModelOnceBuilt = true) where T : class, IEntity, new() {
		EntityBuilder<T> builder = entityBuilderFactory.GetBuilder<T>();
		if (addToModelOnceBuilt) {
			builder.RegisterOnEntityCreated(OnEntityBuilt);
		}

		return builder;
	}

	private void OnEntityBuilt<T>(T entity) where T : class, IEntity, new() {
		entities.Add(entity.UUID, entity);
	}

	public T GetEntity<T>(string id) where T : class, IEntity, new() {
		if (entities.TryGetValue(id, out var entity)) {
			if (entity is T t) {
				return t;
			}
			Debug.LogError($"Entity {id} is not of type {typeof(T)}");
		}
		return null;
	}

	public bool RemoveEntity(string id) {
		if (entities.ContainsKey(id)) {
			IEntity entity = entities[id];
			entity.RecycleToCache();
			entities.Remove(id);
			return true;
		}
		return false;
	}

	public int EntityCount => entities.Count;
}
