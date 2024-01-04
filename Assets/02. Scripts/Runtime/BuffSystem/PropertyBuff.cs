using System;
using System.Collections.Generic;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;

namespace _02._Scripts.Runtime.BuffSystem {
	public abstract class RequiredBuffedProperties {
		public HashSet<BuffTag> BuffTags { get; protected set; }
		public abstract bool ContainBuffedProperties();
	}
	public class RequiredBuffedProperties<T> : RequiredBuffedProperties {
		private HashSet<IBuffedProperty<T>> buffedProperties;
		public HashSet<IBuffedProperty<T>> BuffedProperties => buffedProperties;

		protected void SetBuffedProperties(HashSet<IBuffedProperty<T>> buffedProperties) {
			this.buffedProperties = buffedProperties;
		}

		public override bool ContainBuffedProperties() {
			return buffedProperties.Count > 0;
		}

		public RequiredBuffedProperties(IEntity entity, params BuffTag[] buffTags) {
			BuffTags = new HashSet<BuffTag>(buffTags);
			buffedProperties = new HashSet<IBuffedProperty<T>>();
			SetBuffedProperties(entity.GetBuffedProperties<T>(buffTags));
		}
	}
	
	
	public abstract class PropertyBuff<T> : Buff<T>
		where T : PropertyBuff<T>, new() {
		
		
		
		public override bool Validate() {
			IEnumerable<RequiredBuffedProperties> requiredBuffedPropertiesGroups = GetRequiredBuffedPropertyGroups();
			foreach (var requiredBuffedProperties in requiredBuffedPropertiesGroups) {
				if (!requiredBuffedProperties.ContainBuffedProperties()) {
					return false;
				}
			}

			return true;
		}

		protected abstract IEnumerable<RequiredBuffedProperties> GetRequiredBuffedPropertyGroups();

	}
}