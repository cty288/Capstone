using System;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Properties;
using Runtime.GameResources.Model.Properties.BaitAdjectives;
using Runtime.Utilities;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.GameResources.Model.Base {
	public enum ResourceCategory {
		RawMaterial,
		Bait,
		Item,
		Weapon,
		Currency,
		Skill
	}

	[Serializable]
	public struct ResourcePropertyDescription {
		public string iconName;
		public string localizedDescription;
		
		public ResourcePropertyDescription(string iconName, string localizedDescription) {
			this.iconName = iconName;
			this.localizedDescription = localizedDescription;
		}
		
		
	}
	
	
	public delegate ResourcePropertyDescription GetResourcePropertyDescriptionGetter();
	
	
	public interface IResourceEntity : IEntity, IHaveCustomProperties, IHaveTags {
		public IMaxStack GetMaxStackProperty();

		public void OnPicked();
		
		public ResourceCategory GetResourceCategory();
		
		public string InventoryVCPrefabName { get; }
		
		public string IconSpriteName { get; }
		
		public string OnGroundVCPrefabName { get; }
		
		public string InHandVCPrefabName { get; }
		
		public string DeployedVCPrefabName { get; }
		
		public string AnimLayerName { get; }
		
		public float AnimLayerWeight { get; }
		
		/// <summary>
		/// Width in inventory. Use only 1 or 2. Only effective for weapons.
		/// </summary>
		public int Width { get; }
		
		public List<ResourcePropertyDescription> GetResourcePropertyDescriptions();
		
		public Func<bool> CanInventorySwitchToCondition { get; }
	}
	
	//3 forms
	//on ground
	//in inventory (sprite)
	//in hand

	public abstract class ResourceEntity<T> :  AbstractBasicEntity, IResourceEntity where T : ResourceEntity<T>, new() {
		private IMaxStack maxStackProperty;
		

		[field: ES3Serializable]
		protected bool pickedBefore = false;
		
		//private IStackSize stackSizeProperty;
		protected List<GetResourcePropertyDescriptionGetter> resourcePropertyDescriptionGetters =
			new List<GetResourcePropertyDescriptionGetter>();
		
		private List<ResourcePropertyDescription> resourcePropertyDescriptions = new List<ResourcePropertyDescription>();

		public override void OnAwake() {
			base.OnAwake();
			maxStackProperty = GetProperty<IMaxStack>();
			OnRegisterResourcePropertyDescriptionGetters(ref resourcePropertyDescriptionGetters);
		}

		public virtual void OnRegisterResourcePropertyDescriptionGetters(ref List<GetResourcePropertyDescriptionGetter> list) {
			
		}
		
		
		protected override void OnEntityRegisterAdditionalProperties() {
			RegisterInitialProperty<IMaxStack>(new MaxStack());
			//RegisterInitialProperty<IStackSize>(new StackSize());
		}


		public override void OnDoRecycle() {
			SafeObjectPool<T>.Singleton.Recycle(this as T);
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
			if (pickedBefore) {
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
		
		public void OnPicked() {
			pickedBefore = true;
		}

		public abstract ResourceCategory GetResourceCategory();
		
		[field: ES3Serializable]
		public string InventoryVCPrefabName { get; } = "EntityInventoryVC_Common";

		public string IconSpriteName => $"{EntityName}_Icon";

		public abstract string OnGroundVCPrefabName { get; }
		public virtual string InHandVCPrefabName => OnGroundVCPrefabName;

		public virtual string DeployedVCPrefabName { get; } = null;

		
		public virtual string AnimLayerName => "NoItem";
		public float AnimLayerWeight => 1;

		[field: ES3Serializable]
		public virtual int Width { get; } = 1;

		public List<ResourcePropertyDescription> GetResourcePropertyDescriptions() {
			resourcePropertyDescriptions.Clear();
			foreach (var getter in resourcePropertyDescriptionGetters) {
				resourcePropertyDescriptions.Add(getter());
			}

			return resourcePropertyDescriptions;
		}

		public Func<bool> CanInventorySwitchToCondition { get; } = null;
	}

}