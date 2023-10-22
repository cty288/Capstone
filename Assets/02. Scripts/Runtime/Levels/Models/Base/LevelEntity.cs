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
	
	public class LevelOnExitUnRegister : IUnRegister
	{
		private Action<ILevelEntity> onExit;

		private ILevelEntity level;
		
		public LevelOnExitUnRegister(ILevelEntity level, Action<ILevelEntity> onExit) {
			this.onExit = onExit;
			this.level = level;
		}

		public void UnRegister() {
			level.UnRegisterOnLevelExit(onExit);
			onExit = null;
		}
	}
	public interface ILevelEntity : IEntity, IHaveCustomProperties, IHaveTags {
		public List<LevelSpawnCard> GetAllCardsUnderCost(float cost);
		
		public List<LevelSpawnCard> GetAllCardsUnderCost(float cost, Predicate<LevelSpawnCard> furtherPredicate);
		
		public List<LevelSpawnCard> GetAllNormalEnemiesUnderCost(float cost);
		
		public List<LevelSpawnCard> GetAllNormalEnemiesUnderCost(float cost, Predicate<LevelSpawnCard> furtherPredicate);
		
		public List<LevelSpawnCard> GetAllBosses(Predicate<IEnemyEntity> furtherPredicate);

		public List<LevelSpawnCard> GetAllBosses();

		public List<LevelSpawnCard> GetAllCards();

		public List<LevelSpawnCard> GetCards(Predicate<LevelSpawnCard> predicate);
		
		public int GetCurrentLevelCount();
		
		public int GetMaxEnemyCount();
		
		public BindableProperty<bool> IsInBossFight { get; }
		
		public void SetInBattle(bool isInBattle);
		
		public int CurrentEnemyCount { get; set; }
		
		public void OnLevelExit();
		
		public IUnRegister RegisterOnLevelExit(Action<ILevelEntity> onLevelExit);
		
		public void UnRegisterOnLevelExit(Action<ILevelEntity> onLevelExit);
		
		public Dictionary<Type, LevelExitCondition> LevelExitConditions { get;}
		
		public void AddLevelExitCondition(LevelExitCondition levelExitCondition);
		
		
	}
	
	public abstract class LevelEntity<T> : AbstractBasicEntity, ILevelEntity where T : LevelEntity<T>, new() {
		
		private ISpawnCardsProperty spawnCardsProperty;
		private IMaxEnemiesProperty maxEnemiesProperty;
		[field: ES3Serializable]
		private bool isInBattle = false;
		
		[field: ES3Serializable]
		public int CurrentEnemyCount { get; set; }

		public void OnLevelExit() {
			onLevelExit?.Invoke(this);
		}

		public IUnRegister RegisterOnLevelExit(Action<ILevelEntity> onLevelExit) {
			this.onLevelExit += onLevelExit;
			return new LevelOnExitUnRegister(this, onLevelExit);
		}

		public void UnRegisterOnLevelExit(Action<ILevelEntity> onLevelExit) {
			this.onLevelExit -= onLevelExit;
		}

		[field: ES3Serializable]
		public Dictionary<Type, LevelExitCondition> LevelExitConditions { get; protected set; } 
			= new Dictionary<Type, LevelExitCondition>();

		public void AddLevelExitCondition(LevelExitCondition levelExitCondition) {
			LevelExitConditions.TryAdd(levelExitCondition.GetType(), levelExitCondition);
		}

		protected Action<ILevelEntity> onLevelExit;

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

		public override void OnAwake() {
			base.OnAwake();
			spawnCardsProperty = GetProperty<ISpawnCardsProperty>();
			maxEnemiesProperty = GetProperty<IMaxEnemiesProperty>();
		}

		protected int GetMinRarity(LevelSpawnCard card) {
			return card.MinRarity;
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

		public List<LevelSpawnCard> GetAllBosses(Predicate<IEnemyEntity> templateEntityFurtherPredicate) {
			if (templateEntityFurtherPredicate == null) {
				return GetCards((card => !card.IsNormalEnemy));
			}
			return GetCards((card => !card.IsNormalEnemy && templateEntityFurtherPredicate(card.TemplateEntity)));
		}

		public List<LevelSpawnCard> GetAllBosses() {
			return GetCards((card => !card.IsNormalEnemy));
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
		
		
		[field: SerializeField]
		public BindableProperty<bool> IsInBossFight { get; } = new BindableProperty<bool>();

		

		public void SetInBattle(bool isInBattle) {
			this.isInBattle = isInBattle;
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			this.RegisterInitialProperty<IMaxEnemiesProperty>(new MaxEnemies());
			this.RegisterInitialProperty<ISpawnCardsProperty>(new SpawnCardsProperty());
		}

		public override void OnRecycle() {
			isInBattle = false;
			CurrentEnemyCount = 0;
		}
	}
}