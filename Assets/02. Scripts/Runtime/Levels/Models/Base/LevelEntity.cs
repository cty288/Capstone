using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Levels.Models.Properties;
using _02._Scripts.Runtime.Levels.ViewControllers;
using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;

namespace _02._Scripts.Runtime.Levels.Models {
	public interface ILevelEntity : IEntity, IHaveCustomProperties, IHaveTags {
		public List<LevelSpawnCard> GetAllCardsUnderCost(int cost);
		
		public List<LevelSpawnCard> GetAllNormalEnemiesUnderCost(int cost);

		public List<LevelSpawnCard> GetAllBosses();

		public List<LevelSpawnCard> GetAllCards();

		public List<LevelSpawnCard> GetCards(Predicate<LevelSpawnCard> predicate);
		
		public int GetCurrentLevelCount();
	}
	
	public abstract class LevelEntity<T> : AbstractBasicEntity, ILevelEntity where T : LevelEntity<T>, new() {
		
		private ISpawnCardsProperty spawnCardsProperty;
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
		}

		protected int GetMinRarity(LevelSpawnCard card) {
			return card.MinRarity;
		}
		
		public List<LevelSpawnCard> GetAllCardsUnderCost(int cost) {
			List<LevelSpawnCard> cards = new List<LevelSpawnCard>();
			int level = GetCurrentLevelCount();
			foreach (var card in spawnCardsProperty.RealValues) {
				
				if (card.GetRealSpawnCost(level, GetMinRarity(card)) <= cost) {
					cards.Add(card);
				}
			}
			return cards;
		}

		public List<LevelSpawnCard> GetAllNormalEnemiesUnderCost(int cost) {
			return GetCards((card =>
				card.GetRealSpawnCost(GetCurrentLevelCount(), GetMinRarity(card)) <= cost && card.IsNormalEnemy));
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

		protected override void OnEntityRegisterAdditionalProperties() {
			this.RegisterInitialProperty<IMaxEnemiesProperty>(new MaxEnemies());
			this.RegisterInitialProperty<ISpawnCardsProperty>(new SpawnCardsProperty());
		}

		public override void OnRecycle() {
		
		}
	}
}