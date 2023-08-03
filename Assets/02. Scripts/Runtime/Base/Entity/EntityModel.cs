using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Base.Entity;
using _02._Scripts.Runtime.Common.Entities.Enemies;
using MikroFramework.Architecture;
using UnityEngine;


/// <summary>
/// This is the only place where all entities are saved, no reference to any entity is allowed outside this class
/// To reference an entity, use the entity's id
/// </summary>
public interface IEntityModel: IModel {
	
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

	/// <summary>
	/// Get the enemy builder for the entity type
	/// </summary>
	/// <param name="rarity"></param>
	/// <param name="addToModelOnceBuilt"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	EnemyBuilder<T> GetEnemyBuilder<T>(int rarity, bool addToModelOnceBuilt = true)
		where T : class, IEnemyEntity, new();

	//public EnemyBuilder<T> GetEnemyBuilder<T>(bool addToModelOnceBuilt = true) where T : class, IEnemyEntity, new();

	public T GetEntity<T>(string id) where T : class, IEntity, new();
	
	public bool RemoveEntity(string id);
	
	public int EntityCount { get; }
}
public class EntityModel : AbstractSavableModel, IEntityModel {
	private IEntityBuilderFactory entityBuilderFactory;
	[field: ES3Serializable]
	private Dictionary<string, IEntity> entities = new Dictionary<string, IEntity>();

	[field: ES3Serializable]
	private int test = 5;

	protected override void OnInit() {
		base.OnInit();
		entityBuilderFactory = new EntityBuilderFactory();
		foreach (IEntity entity in entities.Values) {
			entity.OnLoadFromSave();
		}
	}
	
	public TBuilder GetBuilder<TBuilder, TEntity>(int rarity, bool addToModelOnceBuilt = true) where TBuilder : EntityBuilder<TBuilder, TEntity> where TEntity : class, IEntity, new() {
		TBuilder builder = entityBuilderFactory.GetBuilder<TBuilder, TEntity>(rarity);
		if (addToModelOnceBuilt) {
			builder.RegisterOnEntityCreated(OnEntityBuilt);
		}

		return builder;
	}

	/*public override void OnSave() {
		base.OnSave();
		 ES3.Save("entities", entities, "entities", new ES3Settings(){format = ES3.Format.JSON, prettyPrint = true});
	}

	public override void OnLoad() {
		base.OnLoad();
		test++;
		entities = ES3.Load<Dictionary<string, IEntity>>("entities", "entities", new Dictionary<string, IEntity>());
	}*/

	public BasicEntityBuilder<TEntity> GetBuilder<TEntity>(int rarity, bool addToModelOnceBuilt = true) where TEntity : class, IEntity, new() {
		BasicEntityBuilder<TEntity> builder = entityBuilderFactory.GetBuilder<BasicEntityBuilder<TEntity>, TEntity>(rarity);
		
		if (addToModelOnceBuilt) {
			builder.RegisterOnEntityCreated(OnEntityBuilt);
		}

		return builder;
	}

	public EnemyBuilder<T> GetEnemyBuilder<T>(int rarity, bool addToModelOnceBuilt = true) where T : class, IEnemyEntity, new() {
		EnemyBuilder<T> builder = entityBuilderFactory.GetBuilder<EnemyBuilder<T>, T>(rarity);
		
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
