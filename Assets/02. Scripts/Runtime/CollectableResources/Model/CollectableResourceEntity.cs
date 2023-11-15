using System.Collections.Generic;
using _02._Scripts.Runtime.CollectableResources.Model.Properties;
using _02._Scripts.Runtime.Levels;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities.ConfigSheet;

namespace _02._Scripts.Runtime.CollectableResources.Model {
	public interface ICollectableEntity : IEntity, IHaveCustomProperties, IHaveTags, IHaveItemDropCollection {
		ICollectableResourceCurrencyList GetCollectableResourceCurrencyList();
	}
	
	public abstract class CollectableResourceEntity<T> : AbstractBasicEntity, ICollectableEntity
		where T : CollectableResourceEntity<T>, new() {
		protected ILevelNumberProperty levelNumberProperty;
		protected ICollectableResourceCurrencyList collectableResourceCurrencyList;
		protected IItemDropCollectionsProperty itemDropCollectionsProperty;
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.CollectableResourceConfigTable;
		}

		public override void OnAwake() {
			base.OnAwake();
			levelNumberProperty = GetProperty<ILevelNumberProperty>();
			collectableResourceCurrencyList = GetProperty<ICollectableResourceCurrencyList>();
			itemDropCollectionsProperty = GetProperty<IItemDropCollectionsProperty>();
		}

		public ICollectableResourceCurrencyList GetCollectableResourceCurrencyList() {
			return collectableResourceCurrencyList;
		}

		public override void OnDoRecycle() {
			SafeObjectPool<T>.Singleton.Recycle(this as T);
		}

		
		protected override string OnGetDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}

		protected override void OnInitModifiers(int rarity) {
			int level = levelNumberProperty.BaseValue;
			OnInitModifiers(level, rarity);
			SetGeneralAbilityModifier<List<CollectableResourceCurrencyInfo>>(
				new PropertyNameInfo(PropertyName.collectable_resource_currency_list), level);
		}
		
		protected void SetGeneralAbilityModifier<T>(PropertyNameInfo propertyName, int level, bool inverse = false) {
			SetPropertyModifier<T>(propertyName,
				GlobalLevelFormulas.GetGeneralEnemyAbilityModifier<T>(() => 1, () => level, inverse));
		}
		
		protected abstract void OnInitModifiers(int level, int rarity = 1);
		protected override void OnEntityRegisterAdditionalProperties() {
			RegisterInitialProperty<ILevelNumberProperty>(new LevelNumber());
			RegisterInitialProperty<ICollectableResourceCurrencyList>(new CollectableResourceCurrencyList());
			RegisterInitialProperty<IItemDropCollectionsProperty>(new ItemDropCollections());
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}

		public IItemDropCollectionsProperty GetItemDropCollectionsProperty() {
			return itemDropCollectionsProperty;
		}
	}
}