using System;
using _02._Scripts.Runtime.BuffSystem;
using Runtime.DataFramework.Properties;

namespace Runtime.GameResources.Others {
	[Serializable]
	public class ResourcePropertyDescription {
		
		public string iconName;
		private string localizedDescription;
		public bool display;
		public string LocalizedPropertyName { get; set; }
		
		public ResourcePropertyDescription(string iconName, string localizedPropertyName,
			string localizedDescription, bool display = true) {
			this.iconName = iconName;
			this.localizedDescription = localizedDescription;
			this.LocalizedPropertyName = localizedPropertyName;
			this.display = display;
		}
		
		public ResourcePropertyDescription(){}
		
		public virtual string GetLocalizedDescription(bool compareBuffedProperties = false) {
			return localizedDescription;
		}
	}

	public class WeaponBuffedAdditionalPropertyDescription : ResourcePropertyDescription {
		public override string GetLocalizedDescription(bool compareBuffedProperties = false) {
			return $"<color=#00C531>{base.GetLocalizedDescription(compareBuffedProperties)}</color>";
		}
		
		public WeaponBuffedAdditionalPropertyDescription(string iconName, string localizedPropertyName,
			string localizedDescription, bool display = true) : base(iconName, localizedPropertyName, localizedDescription, display) {
		}
	}

	/*public enum BuffedPropertyComparison {
		Stronger,
		Weaker,
		Equal,
	}*/
	
	[Serializable]
	public class ResourceBuffedPropertyDescription<T> : ResourcePropertyDescription {
		protected IBuffedProperty<T> buffedProperty;
		protected Func<T, string> localizedPropertyValueGetter;
		protected Func<T, T, int> comparison;
		protected Func<(T, T)> comparisonGetter;

		public ResourceBuffedPropertyDescription
			(IBuffedProperty<T> buffedProperty, string iconName, string localizedPropertyName, 
				Func<T, string> localizedPropertyValueGetter, 
				Func<T, T, int> comparison,
				bool display = true) {
			
			this.buffedProperty = buffedProperty;
			this.iconName = iconName;
			this.LocalizedPropertyName = localizedPropertyName;
			this.localizedPropertyValueGetter = localizedPropertyValueGetter;
			this.display = display;
			this.comparison = comparison;
			comparisonGetter = () => (buffedProperty.InitialValue, buffedProperty.RealValue);
		}

		public override string GetLocalizedDescription(bool compareBuffedProperties = false) {
			if (!compareBuffedProperties || !buffedProperty.GetIsBuffed()) {
				return localizedPropertyValueGetter(buffedProperty.RealValue);
			}
			else {
				//T initialValue = buffedProperty.InitialValue;
				//T realValue = buffedProperty.RealValue;

				(T initialValue, T realValue) = comparisonGetter();
				
				int comparison = this.comparison(initialValue, realValue);
				
				string initialValueDescription = localizedPropertyValueGetter(initialValue);
				string realValueDescription = localizedPropertyValueGetter(realValue);
				string color = "";
				/*switch (comparison) {
					case BuffedPropertyComparison.Stronger:
						color = "green";
						break;
					case BuffedPropertyComparison.Weaker:
						color = "red";
						break;
					case BuffedPropertyComparison.Equal:
						color = "black";
						break;
				}*/
				
				if (comparison > 0) {
					color = "#00C531";
				}
				else if (comparison < 0) {
					color = "red";
				}
				else {
					color = "black";
				}

				if (comparison > 0) {
					return $"{initialValueDescription}  <sprite index=0>  <color={color}><b><size=110%>{realValueDescription}</b></size></color>";
				}else if (comparison < 0) {
					return $"{initialValueDescription}  <sprite index=0>  <color={color}><b><size=90%>{realValueDescription}</b></size></color>";
				}
				else {
					return realValueDescription;
				}
				
			}
		}
	}

}