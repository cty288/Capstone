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
		Weapon
	}
	public interface IResourceEntity : IEntity, IHaveCustomProperties, IHaveTags {
		public IMaxStack GetMaxStackProperty();

		public void OnPicked();
		
		public ResourceCategory GetResourceCategory();
		//public string 
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
		protected override void OnEntityStart() {
			base.OnEntityStart();
			
			maxStackProperty = GetProperty<IMaxStack>();
			//stackSizeProperty = GetProperty<IStackSize>();
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
	}

}