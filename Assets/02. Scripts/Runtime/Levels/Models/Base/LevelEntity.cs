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
		// public List<LevelSpawnCard> GetAllCardsUnderCost(float cost);
		
		// public List<LevelSpawnCard> GetAllCardsUnderCost(float cost, Predicate<LevelSpawnCard> furtherPredicate);
		
		// public List<LevelSpawnCard> GetAllNormalEnemiesUnderCost(float cost);
		
		// public List<LevelSpawnCard> GetAllNormalEnemiesUnderCost(float cost, Predicate<LevelSpawnCard> furtherPredicate);
		
		public List<LevelSpawnCard> GetAllBosses(Predicate<IEnemyEntity> furtherPredicate);

		public List<LevelSpawnCard> GetAllBosses();

		public List<LevelSpawnCard[]> GetAllCards();

		public List<LevelSpawnCard[]> GetCards(Predicate<LevelSpawnCard[]> predicate);
		
		public int GetCurrentLevelCount();
		
		// public int GetMaxEnemyCount();
		
		public BindableProperty<bool> IsInBossFight { get; }
		
		public int DayStayed { get; set; }
		
		public void SetInBattle(bool isInBattle);
		
		// public int CurrentEnemyCount { get; set; }
		
		public void OnLevelExit();
		
		public IUnRegister RegisterOnLevelExit(Action<ILevelEntity> onLevelExit);
		
		public void UnRegisterOnLevelExit(Action<ILevelEntity> onLevelExit);
		
		public Dictionary<Type, LevelExitCondition> LevelExitConditions { get;}
		
		public void AddLevelExitCondition(LevelExitCondition levelExitCondition);
		public HashSet<ISubAreaLevelEntity> GetAllSubAreaLevels();

		public void AddSubArea(string uuid);
	}
	
	public abstract class LevelEntity<T> : AbstractBasicEntity, ILevelEntity where T : LevelEntity<T>, new() {
		
		protected ISpawnCardsProperty spawnCardsProperty;
		//protected ISubAreaLevelsProperty subAreaLevelsProperty;
		// protected IMaxEnemiesProperty maxEnemiesProperty;
		[field: ES3Serializable]
		private bool isInBattle = false;
		
		[field: ES3Serializable]
		private HashSet<string> SubAreaUUIDs { get; set; } = new HashSet<string>();
		
		private HashSet<ISubAreaLevelEntity> subAreaLevelEntities = new HashSet<ISubAreaLevelEntity>();
		
		// [field: ES3Serializable]
		// public int CurrentEnemyCount { get; set; }

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
			//subAreaLevelsProperty = GetProperty<ISubAreaLevelsProperty>();
			// maxEnemiesProperty = GetProperty<IMaxEnemiesProperty>();
		}

		protected int GetMinRarity(LevelSpawnCard card) {
			return card.MinRarity;
		}
		
		private bool IsNormalEnemies(LevelSpawnCard[] cards)
		{
			return cards.Any(card => card.IsNormalEnemy == false);
		}
		
		public List<LevelSpawnCard> GetAllBosses(Predicate<IEnemyEntity> templateEntityFurtherPredicate)
		{
			List<LevelSpawnCard> bossCards = new List<LevelSpawnCard>();
			if (templateEntityFurtherPredicate == null)
			{
				GetCards((cards => !cards[0].IsNormalEnemy))
					.ForEach(cards => bossCards.Add(cards[0]));
				return bossCards;
			}
			GetCards((cards => !cards[0].IsNormalEnemy && templateEntityFurtherPredicate(cards[0].TemplateEntity)))
				.ForEach(cards => bossCards.Add(cards[0]));
			return bossCards;
		}

		public List<LevelSpawnCard> GetAllBosses() {
			List<LevelSpawnCard> bossCards = new List<LevelSpawnCard>();
			GetCards((cards => !cards[0].IsNormalEnemy))
				.ForEach(cards => bossCards.Add(cards[0]));
			return bossCards;
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

		// public int GetMaxEnemyCount() {
		// 	return maxEnemiesProperty.RealValue;
		// }
		
		public HashSet<ISubAreaLevelEntity> GetAllSubAreaLevels() {
			if (subAreaLevelEntities.Count != SubAreaUUIDs.Count) {
				subAreaLevelEntities.Clear();
				
				foreach (var uuid in SubAreaUUIDs) {
					subAreaLevelEntities.Add(GlobalEntities.GetEntityAndModel(uuid).Item1 as ISubAreaLevelEntity);
				}
			}

			return subAreaLevelEntities;
		}

		public void AddSubArea(string uuid) {
			SubAreaUUIDs.Add(uuid);
			subAreaLevelEntities.Add(GlobalEntities.GetEntityAndModel(uuid).Item1 as ISubAreaLevelEntity);
		}


		[field: SerializeField]
		public BindableProperty<bool> IsInBossFight { get; } = new BindableProperty<bool>();

		[field: ES3Serializable] 
		public int DayStayed { get; set; } = 0;


		public void SetInBattle(bool isInBattle) {
			this.isInBattle = isInBattle;
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			this.RegisterInitialProperty<IMaxEnemiesProperty>(new MaxEnemiesProperty());
			this.RegisterInitialProperty<ISpawnCardsProperty>(new SpawnCardsProperty());
		
		}

		public override void OnRecycle() {
			
			isInBattle = false;
			// CurrentEnemyCount = 0;
			LevelExitConditions.Clear();
			onLevelExit = null;
			IsInBossFight.Value = false;
			SubAreaUUIDs.Clear();
			subAreaLevelEntities.Clear();
			DayStayed = 0;
		}
	}
}