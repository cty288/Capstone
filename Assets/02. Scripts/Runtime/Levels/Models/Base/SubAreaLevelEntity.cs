﻿using System;
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
        public List<LevelSpawnCard> GetAllCardsUnderCost(float cost);
		
		public List<LevelSpawnCard> GetAllCardsUnderCost(float cost, Predicate<LevelSpawnCard> furtherPredicate);
		
		public List<LevelSpawnCard> GetAllNormalEnemiesUnderCost(float cost);
		
		public List<LevelSpawnCard> GetAllNormalEnemiesUnderCost(float cost, Predicate<LevelSpawnCard> furtherPredicate);
		
		public List<LevelSpawnCard> GetAllCards();

		public List<LevelSpawnCard> GetCards(Predicate<LevelSpawnCard> predicate);
		
		public int GetCurrentLevelCount();
		
		public int GetMaxEnemyCount();
		
		public int GetSubAreaNavMeshModifier();
		
		public bool IsActiveSpawner { get; set; }
		
		public int CurrentEnemyCount { get; set; }
		public int TotalEnemiesSpawnedSinceOffCooldown { get; set; }
		
		public SubAreaDangerLevel GetSpawnStatus();
	}
	
	public abstract class SubAreaLevelEntity<T> : AbstractBasicEntity, ISubAreaLevelEntity where T : SubAreaLevelEntity<T>, new() {
		
		private ISpawnCardsProperty spawnCardsProperty;
		private IMaxEnemiesProperty maxEnemiesProperty;
		private ISubAreaNavMeshModifier subAreaNavMeshModifier;
		
		public bool IsActiveSpawner { get; set; }
		
		[field: ES3Serializable]
		public int CurrentEnemyCount { get; set; }
		
		[field: ES3Serializable]
		public int TotalEnemiesSpawnedSinceOffCooldown { get; set; }
		
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
			subAreaNavMeshModifier = GetProperty<ISubAreaNavMeshModifier>();
		}
        
		protected int GetMinRarity(LevelSpawnCard card) {
			return card.MinRarity;
		}
		
		public SubAreaDangerLevel GetSpawnStatus()
		{
			if(IsActiveSpawner)
				return SubAreaDangerLevel.Medium;
			else
				return SubAreaDangerLevel.Safe;
		}
		
		public List<LevelSpawnCard> GetAllCardsUnderCost(float cost) {
			List<LevelSpawnCard> cards = new List<LevelSpawnCard>();
			int level = GetCurrentLevelCount();
			foreach (var card in spawnCardsProperty.RealValues) {
				
				if (card.GetRealSpawnCost(level, GetMinRarity(card)) <= cost) {
					cards.Add(card);
				}
			}
			return cards;
		}

		public List<LevelSpawnCard> GetAllCardsUnderCost(float cost, Predicate<LevelSpawnCard> furtherPredicate) {
			List<LevelSpawnCard> cards = new List<LevelSpawnCard>();
			int level = GetCurrentLevelCount();
			foreach (var card in spawnCardsProperty.RealValues) {
				if (card.GetRealSpawnCost(level, GetMinRarity(card)) <= cost && furtherPredicate(card)) {
					cards.Add(card);
				}
			}
			return cards;
		}

		public List<LevelSpawnCard> GetAllNormalEnemiesUnderCost(float cost) {
			return GetCards((card =>
				card.GetRealSpawnCost(GetCurrentLevelCount(), GetMinRarity(card)) <= cost && card.IsNormalEnemy));
		}

		public List<LevelSpawnCard> GetAllNormalEnemiesUnderCost(float cost, Predicate<LevelSpawnCard> furtherPredicate) {
			return GetCards((card =>
				card.GetRealSpawnCost(GetCurrentLevelCount(), GetMinRarity(card)) <= cost && card.IsNormalEnemy &&
				furtherPredicate(card)));
		}

		public List<LevelSpawnCard> GetAllCards() {
			return spawnCardsProperty.RealValues.ToList();
		}
		
		public List<LevelSpawnCard> GetCards(Predicate<LevelSpawnCard> predicate) {
			List<LevelSpawnCard> cards = new List<LevelSpawnCard>();
			foreach (var card in spawnCardsProperty.RealValues) {
				if (predicate(card)) {
					cards.Add(card);
				}
			}
			return cards;
		}

		public int GetCurrentLevelCount() {
			return GetRarity();
		}

		public int GetMaxEnemyCount() {
			return maxEnemiesProperty.RealValue;
		}

		public int GetSubAreaNavMeshModifier()
		{
			return subAreaNavMeshModifier.RealValue;
		}
        
		protected override void OnEntityRegisterAdditionalProperties() {
			this.RegisterInitialProperty<IMaxEnemiesProperty>(new MaxEnemies());
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