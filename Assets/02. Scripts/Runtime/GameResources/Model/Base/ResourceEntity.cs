using System;
using System.Collections.Generic;
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
	
	public interface IResourceEntity : IEntity, IHaveCustomProperties, IHaveTags {
		public IBaitAdjectives GetBaitAdjectivesProperty();
		
		public IMaxStack GetMaxStackProperty();
	}
	
	public abstract class ResourceEntity<T> :  AbstractBasicEntity, IResourceEntity where T : ResourceEntity<T>, new() {
		
		private IBaitAdjectives baitAdjectivesProperty;
		private IMaxStack maxStackProperty;
		protected override void OnEntityStart() {
			base.OnEntityStart();
			baitAdjectivesProperty = GetProperty<IBaitAdjectives>();
			maxStackProperty = GetProperty<IMaxStack>();
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			RegisterInitialProperty<IBaitAdjectives>(new BaitAdjectives());
			RegisterInitialProperty<IMaxStack>(new MaxStack());
		}


		public override void OnDoRecycle() {
			SafeObjectPool<T>.Singleton.Recycle(this as T);
		}
		
		/// <summary>
		/// The display name of the resource entity is the bait adjectives + the name of the resource entity <br />
		/// e.g. "Fresh" + "Dark" + "Mushroom" = "Fresh, Dark Mushroom"
		/// </summary>
		/// <returns></returns>
		public override string GetDisplayName() {
			string selfDisplayName = base.GetDisplayName();
			string description = baitAdjectivesProperty.GetDescription();
			if (String.IsNullOrEmpty(description)) {
				return selfDisplayName;
			}
			return description + Localization.GetJoin() + selfDisplayName;
		}

		public IBaitAdjectives GetBaitAdjectivesProperty() {
			return baitAdjectivesProperty;
		}

		public IMaxStack GetMaxStackProperty() {
			return maxStackProperty;
		}
	}

}