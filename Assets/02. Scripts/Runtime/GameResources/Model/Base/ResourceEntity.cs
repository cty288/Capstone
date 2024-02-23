using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Properties;
using Runtime.GameResources.Model.Properties.BaitAdjectives;
using Runtime.GameResources.Others;
using Runtime.Utilities;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.GameResources.Model.Base {
	public enum ResourceCategory {
		RawMaterial,
		Bait,
		Item,
		Weapon,
		Currency,
		Skill,
		WeaponParts,
		All // do not use this
	}

	
	
	public delegate ResourcePropertyDescription GetResourcePropertyDescriptionGetter();
	
	
	public interface IResourceEntity : IEntity, IHaveCustomProperties, IHaveTags {
		public IMaxStack GetMaxStackProperty();

		public void OnAddedToSlot();

		public void OnAddedToInventory(string playerUUID);
		
		public void OnRemovedFromInventory();
		
		public ResourceCategory GetResourceCategory();
		
		public string InventoryVCPrefabName { get; }
		
		//public string IconSpriteName { get; }
		
		public string OnGroundVCPrefabName { get; }
		
		public string InHandVCPrefabName { get; }
		
		public string DeployedVCPrefabName { get; }
		
		/// <summary>
		/// Can this resourced be rewarded by the game through normal means?
		/// </summary>
		public bool Collectable { get; }
		
		/// <summary>
		/// Width in inventory. Use only 1 or 2. Only effective for weapons.
		/// </summary>
		public int Width { get; }
		
		public List<ResourcePropertyDescription> GetResourcePropertyDescriptions();
		
		public Func<Dictionary<CurrencyType, int>, bool> CanInventorySwitchToCondition { get; }
		
		public IResourceEntity GetReturnToBaseEntity();
		
		public void AddAdditionalResourcePropertyDescriptionGetters(List<GetResourcePropertyDescriptionGetter> list);
		
		public void RemoveAdditionalResourcePropertyDescriptionGetters(List<GetResourcePropertyDescriptionGetter> list);
		
		/*public void OnStartHold();
		
		public void OnStopHold();*/
		
		public BindableProperty<bool> IsHolding { get; }
	}
	

	
	//3 forms
	//on ground
	//in inventory (sprite)
	//in hand

	public abstract class ResourceEntity<T> :  AbstractBasicEntity, IResourceEntity where T : ResourceEntity<T>, new() {
		private IMaxStack maxStackProperty;
		

		[field: ES3Serializable]
		protected bool encounteredBefore = false;
		
		//private IStackSize stackSizeProperty;
		protected List<GetResourcePropertyDescriptionGetter> resourcePropertyDescriptionGetters =
			new List<GetResourcePropertyDescriptionGetter>();
		protected HashSet<List<GetResourcePropertyDescriptionGetter>> additionalResourcePropertyDescriptionGetters =
			new HashSet<List<GetResourcePropertyDescriptionGetter>>();

		private List<ResourcePropertyDescription> resourcePropertyDescriptions = new List<ResourcePropertyDescription>();
		
		[field: ES3NonSerializable]
		public BindableProperty<bool> IsHolding { get; private set; } = new BindableProperty<bool>(false);
		
		[field: ES3Serializable]
		protected bool isInInventory = false;
		public override void OnAwake() {
			base.OnAwake();
			OnResourceAwake();
			maxStackProperty = GetProperty<IMaxStack>();
		}

		public virtual void OnResourceAwake() {
			
		}
		public virtual void OnRegisterResourcePropertyDescriptionGetters(ref List<GetResourcePropertyDescriptionGetter> list) {
			
		}
		
		
		protected override void OnEntityRegisterAdditionalProperties() {
			RegisterInitialProperty<IMaxStack>(new MaxStack());
			//RegisterInitialProperty<IStackSize>(new StackSize());
		}


		public override void OnDoRecycle() {
			encounteredBefore = false;
			resourcePropertyDescriptionGetters?.Clear();
			IsHolding.Value = false;
			
			SafeObjectPool<T>.Singleton.Recycle(this as T);
			
			isInInventory = false;
		}
		
		/// <summary>
		/// The display name of the resource entity is the bait adjectives + the name of the resource entity <br />
		/// e.g. "Fresh" + "Dark" + "Mushroom" = "Fresh, Dark Mushroom"
		/// </summary>
		/// <returns></returns>
		/*public override string GetDisplayName() {
			string selfDisplayName = base.GetDisplayName();
			string description = baitAdjectivesProperty.GetDescription();
			if (String.IsNullOrEmpty(description)) {
				return selfDisplayName;
			}
			return description + Localization.GetJoin() + selfDisplayName;
		}*/


		public IMaxStack GetMaxStackProperty() {
			return maxStackProperty;
		}


		public override string GetDisplayName() {
			string originalDisplayName = base.GetDisplayName();
			if (encounteredBefore) {
				return originalDisplayName;
			}
			else {
				string displayName = OnGetDisplayNameBeforeFirstPicked(originalDisplayName);
				return displayName;
			}
		}

		protected abstract string OnGetDisplayNameBeforeFirstPicked(string originalDisplayName);
		
		/*public IStackSize GetStackSizeProperty() {
			return stackSizeProperty;
		}*/
		
		public void OnAddedToSlot() {
			encounteredBefore = true;
		}

		public virtual void OnAddedToInventory(string playerUUID) {
			isInInventory = true;
		}

		public virtual void OnRemovedFromInventory() {
			isInInventory = false;
		}

		public abstract ResourceCategory GetResourceCategory();
		
		[field: ES3Serializable]
		public string InventoryVCPrefabName { get; } = "EntityInventoryVC_Common";

		//public string IconSpriteName => $"{EntityName}_Icon";

		public abstract string OnGroundVCPrefabName { get; }
		public virtual string InHandVCPrefabName => OnGroundVCPrefabName;

		public virtual string DeployedVCPrefabName { get; } = null;
		public abstract bool Collectable { get; }


		public override void OnStart(bool isLoadedFromSave) {
			resourcePropertyDescriptionGetters?.Clear();
			OnRegisterResourcePropertyDescriptionGetters(ref resourcePropertyDescriptionGetters);
			base.OnStart(isLoadedFromSave);
		}

		[field: ES3Serializable]
		public virtual int Width { get; } = 1;

		public List<ResourcePropertyDescription> GetResourcePropertyDescriptions() {
			resourcePropertyDescriptions.Clear();
			foreach (var getter in resourcePropertyDescriptionGetters) {
				resourcePropertyDescriptions.Add(getter());
			}
			
			foreach (var additionalGetter in additionalResourcePropertyDescriptionGetters) {
				foreach (var getter in additionalGetter) {
					resourcePropertyDescriptions.Add(getter());
				}
			}

			return resourcePropertyDescriptions;
		}

		public Func<Dictionary<CurrencyType, int>,bool> CanInventorySwitchToCondition { get; } = null;
		public abstract IResourceEntity GetReturnToBaseEntity();
		public void AddAdditionalResourcePropertyDescriptionGetters(List<GetResourcePropertyDescriptionGetter> list) {
			additionalResourcePropertyDescriptionGetters.Add(list);
		}

		public void RemoveAdditionalResourcePropertyDescriptionGetters(List<GetResourcePropertyDescriptionGetter> list) {
			additionalResourcePropertyDescriptionGetters.Remove(list);
		}

		/*public void OnStartHold() {
			IsHolding.Value = true;
		}

		public void OnStopHold() {
			IsHolding.Value = false;
		}*/

		
	
	}

}