using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using _02._Scripts.Runtime.Levels.Models.Properties;
using _02._Scripts.Runtime.Levels.ViewControllers;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Enemies.Model;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.Models {
	public enum SubAreaDangerLevel {
		Safe,
		Low,
		Medium,
		High
	}
	
	public interface ISubAreaLevelEntity : IEntity, IHaveCustomProperties, IHaveTags {
        public List<LevelSpawnCard[]> GetAllCardsUnderCost(float cost);
		
		public List<LevelSpawnCard[]> GetAllCardsUnderCost(float cost, Predicate<LevelSpawnCard[]> furtherPredicate);
		
		public List<LevelSpawnCard[]> GetAllNormalEnemiesUnderCost(float cost);
		
		public List<LevelSpawnCard[]> GetAllNormalEnemiesUnderCost(float cost, Predicate<LevelSpawnCard[]> furtherPredicate);
		
		public List<LevelSpawnCard[]> GetAllCards();

		public List<LevelSpawnCard[]> GetCards(Predicate<LevelSpawnCard[]> predicate);
		
		public int GetCurrentLevelCount();
		
		public int GetMaxEnemyCount();
		
		public Dictionary<string, int> GetMaxSpawnPerEnemy();
		
		public int GetSubAreaNavMeshModifier();
		
		public bool IsActiveSpawner { get; set; }
		
		public int CurrentEnemyCount { get; set; }
		public int TotalEnemiesSpawnedSinceOffCooldown { get; set; }
		public Dictionary<string, int> GetEnemyCountDictionary();
		public void InitializeEnemyCountDictionary();
		public void IncrementEnemyCountDictionary(string name);
		public void DecrementEnemyCountDictionary(string name);
		public void ClearEnemyCountDictionary();
		
		public SubAreaDangerLevel GetSpawnStatus();
	}
	
	public abstract class SubAreaLevelEntity<T> : AbstractBasicEntity, ISubAreaLevelEntity where T : SubAreaLevelEntity<T>, new() {
		
		private ISpawnCardsProperty spawnCardsProperty;
		private IMaxEnemiesProperty maxEnemiesProperty;
		private IMaxSpawnPerEnemyProperty maxSpawnPerEnemyProperty;
		private ISubAreaNavMeshModifier subAreaNavMeshModifier;
		
		public bool IsActiveSpawner { get; set; }
		
		[field: ES3Serializable]
		public int CurrentEnemyCount { get; set; }
		
		[field: ES3Serializable]
		public int TotalEnemiesSpawnedSinceOffCooldown { get; set; }
		
		[field: ES3Serializable]
		public Dictionary<string, int> enemyCount = new Dictionary<string, int>();

		
		protected override void OnEntityStart(bool isLoadedFromSave)
		{
			IsActiveSpawner = true;
		}
		
		protected override string OnGetDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}

		protected override ConfigTable GetConfigTable() {
			return null;
		}

		public override void OnDoRecycle() {
			SafeObjectPool<T>.Singleton.Recycle(this as T);
		}

		public override void OnAwake() {
			base.OnAwake(); 
			spawnCardsProperty = GetProperty<ISpawnCardsProperty>();
			maxEnemiesProperty = GetProperty<IMaxEnemiesProperty>();
			maxSpawnPerEnemyProperty = GetProperty<IMaxSpawnPerEnemyProperty>();
			subAreaNavMeshModifier = GetProperty<ISubAreaNavMeshModifier>();
		}
        
		protected int GetMinRarity(LevelSpawnCard card) {
			return card.MinRarity;
		}

		public Dictionary<string, int> GetEnemyCountDictionary()
		{
			return enemyCount;
		}

		public void InitializeEnemyCountDictionary()
		{
			// Debug.Log("SPAWN_TEST: initializing enemy count dictionary");
			foreach (var enemy in GetMaxSpawnPerEnemy())
			{
				enemyCount.TryAdd(enemy.Key, 0);
			}
			
			// string print = "";
			// foreach (var kv in enemyCount)
			// {
			// 	print += string.Format("Key = {0}, Value = {1} ", kv.Key, kv.Value);
			// }
			// Debug.Log($"SPAWN_TEST: {print}");
		}

		public void IncrementEnemyCountDictionary(string name)
		{
			enemyCount[name]++;
		}

		public void DecrementEnemyCountDictionary(string name)
		{
			enemyCount[name]--;
		}

		public void ClearEnemyCountDictionary()
		{
			if(enemyCount != null)
				enemyCount.Clear();
		}

		public SubAreaDangerLevel GetSpawnStatus()
		{
			if(IsActiveSpawner)
				return SubAreaDangerLevel.Medium;
			else
				return SubAreaDangerLevel.Safe;
		}

		private float GetMinCost(LevelSpawnCard[] cards)
		{
			return cards.Sum(card => card.GetRealSpawnCost(GetCurrentLevelCount(), GetMinRarity(card)));
		}

		private bool IsNormalEnemies(LevelSpawnCard[] cards)
		{
			return cards.All(card => card.IsNormalEnemy);
		}

		private bool IsAllEnemiesUnderCount(LevelSpawnCard[] cards)
		{
			// string print = "";
			// foreach (KeyValuePair<string, int> kvp in enemyCount)
			// {
			// 	print += string.Format("Key = {0}, Value = {1} ", kvp.Key, kvp.Value);
			// }
			//
			// Debug.Log($"SPAWN_TEST: dictionary = {print}");
			return cards.All(card => enemyCount[card.PrefabNames[0].Split('_')[0]] <= GetMaxSpawnPerEnemy()[card.PrefabNames[0].Split('_')[0]]);
		}
		
		public List<LevelSpawnCard[]> GetAllCardsUnderCost(float cost) {
			List<LevelSpawnCard[]> cardsList = new List<LevelSpawnCard[]>();
			foreach (var cards in spawnCardsProperty.RealValues) {
				if (GetMinCost(cards) <= cost) {
					cardsList.Add(cards);
				}
			}
			return cardsList;
		}

		public List<LevelSpawnCard[]> GetAllCardsUnderCost(float cost, Predicate<LevelSpawnCard[]> furtherPredicate) {
			List<LevelSpawnCard[]> cardsList = new List<LevelSpawnCard[]>();
			foreach (var cards in spawnCardsProperty.RealValues) {
				if (GetMinCost(cards) <= cost && furtherPredicate(cards)) {
					cardsList.Add(cards);
				}
			}
			return cardsList;
		}

		public List<LevelSpawnCard[]> GetAllNormalEnemiesUnderCost(float cost) {
			return GetCards((cards =>
				GetMinCost(cards) <= cost 
				&& IsNormalEnemies(cards)
				&& IsAllEnemiesUnderCount(cards)
				));
		}

		public List<LevelSpawnCard[]> GetAllNormalEnemiesUnderCost(float cost, Predicate<LevelSpawnCard[]> furtherPredicate) {
			return GetCards((cards =>
				GetMinCost(cards) <= cost
				&& IsNormalEnemies(cards)
				&& IsAllEnemiesUnderCount(cards)
				&& furtherPredicate(cards)));
		}

		public List<LevelSpawnCard[]> GetAllCards() {
			return spawnCardsProperty.RealValues.ToList();
		}

		public List<LevelSpawnCard[]> GetCards(Predicate<LevelSpawnCard[]> predicate) {
			List<LevelSpawnCard[]> cardsList = new List<LevelSpawnCard[]>();
			foreach (var cards in spawnCardsProperty.RealValues) {
				if (predicate(cards)) {
					cardsList.Add(cards);
				}
			}
			return cardsList;
		}

		public int GetCurrentLevelCount() {
			return GetRarity();
		}

		public Dictionary<string, int> GetMaxSpawnPerEnemy() {
			return maxSpawnPerEnemyProperty.RealValue;
		}
		
		public int GetMaxEnemyCount() {
			return maxEnemiesProperty.RealValue;
		}

		public int GetSubAreaNavMeshModifier()
		{
			return subAreaNavMeshModifier.RealValue;
		}
        
		protected override void OnEntityRegisterAdditionalProperties() {
			this.RegisterInitialProperty<IMaxEnemiesProperty>(new MaxEnemiesProperty());
			this.RegisterInitialProperty<IMaxSpawnPerEnemyProperty>(new MaxSpawnPerEnemyProperty());
			this.RegisterInitialProperty<ISpawnCardsProperty>(new SpawnCardsProperty());
			this.RegisterInitialProperty<ISubAreaNavMeshModifier>(new SubAreaNavMeshModifier());
			// this.RegisterInitialProperty<ISpawnCooldown>(new SpawnCooldown());
		}

		public override void OnRecycle() {
			CurrentEnemyCount = 0;
			TotalEnemiesSpawnedSinceOffCooldown = 0;
			IsActiveSpawner = true;
		}
	}
}