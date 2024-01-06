using System;
using System.Collections.Generic;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;

namespace _02._Scripts.Runtime.BuffSystem {
	public abstract class BuffedProperties {
		public HashSet<BuffTag> BuffTags { get; protected set; }
		public abstract bool ContainBuffedProperties();
		
		public abstract bool IsRequiredBuffedPropertiesValid { get; protected set; }
		
		public abstract void AddBuffCounters();
		public abstract void RemoveBuffCounters();
	}
	public class BuffedProperties<T> : BuffedProperties {
		private HashSet<IBuffedProperty<T>> properties;
		public HashSet<IBuffedProperty<T>> Properties => properties;

		protected void SetBuffedProperties(HashSet<IBuffedProperty<T>> buffedProperties) {
			this.properties = buffedProperties;
		}

		public override bool ContainBuffedProperties() {
			return properties.Count > 0;
		}

		public override bool IsRequiredBuffedPropertiesValid { get; protected set; }
		public override void AddBuffCounters() {
			foreach (var property in properties) {
				property.IsBuffedRC.Retain();
			}
		}

		public override void RemoveBuffCounters() {
			foreach (var property in properties) {
				property.IsBuffedRC.Release();
			}
		}

		public BuffedProperties(IEntity entity, bool isRequired, params BuffTag[] buffTags) {
			BuffTags = new HashSet<BuffTag>(buffTags);
			properties = new HashSet<IBuffedProperty<T>>();
			this.IsRequiredBuffedPropertiesValid = isRequired;
			SetBuffedProperties(entity.GetBuffedProperties<T>(buffTags));
		}
	}
	
	
	public abstract class PropertyBuff<T> : Buff<T>
		where T : PropertyBuff<T>, new() {
		
		
		
		//TODO: set IsBuffed variable
		public override bool Validate() {
			IEnumerable<BuffedProperties> requiredBuffedPropertiesGroups = GetBuffedPropertyGroups();
			foreach (var requiredBuffedProperties in requiredBuffedPropertiesGroups) {
				if (requiredBuffedProperties.IsRequiredBuffedPropertiesValid && !requiredBuffedProperties.ContainBuffedProperties()) {
					return false;
				}
			}

			return true;
		}

		public override void OnAwake() {
			base.OnAwake();
			IEnumerable<BuffedProperties> requiredBuffedPropertiesGroups = GetBuffedPropertyGroups();
			foreach (var requiredBuffedProperties in requiredBuffedPropertiesGroups) {
				requiredBuffedProperties.AddBuffCounters();
			}
		}

		public override void OnEnd() {
			IEnumerable<BuffedProperties> requiredBuffedPropertiesGroups = GetBuffedPropertyGroups();
			foreach (var requiredBuffedProperties in requiredBuffedPropertiesGroups) {
				requiredBuffedProperties.RemoveBuffCounters();
			}
		}

		protected abstract IEnumerable<BuffedProperties> GetBuffedPropertyGroups();

	}
}