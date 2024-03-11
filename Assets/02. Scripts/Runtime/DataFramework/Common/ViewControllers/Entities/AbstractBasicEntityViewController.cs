using System;
using System.Reflection;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using UnityEngine;

namespace Runtime.DataFramework.ViewControllers.Entities {
	public abstract class AbstractBasicEntityViewController<T>: AbstractEntityViewController<T> 
		where T : class, IHaveCustomProperties, IHaveTags {
		
		protected void BindCustomData<T>(string bindedPropertyName, string customPropertyName, string customDataName,
			Action<T, T?> callback = null) {
			IBindableProperty dataProperty = BoundEntity.GetCustomDataValue(customPropertyName, customDataName);
			if(dataProperty == null) {
				
				throw new Exception(
					$"Custom data property {customDataName} doesn't exist in custom property {customPropertyName}");
			}

			Bind<T>(bindedPropertyName, dataProperty, property => property, callback);
		}


		protected void BindCustomData<TargetType>(string bindedPropertyName, string customPropertyName, string customDataName,
			Func<dynamic, TargetType> getter, Action<TargetType, TargetType> callback = null) {
			IBindableProperty dataProperty = BoundEntity.GetCustomDataValue(customPropertyName, customDataName);
			if(dataProperty == null) {
				throw new Exception(
					$"Custom data property {customDataName} doesn't exist in custom property {customPropertyName}");
			}
			
			PropertyInfo bindedProperty = GetType().GetProperty(bindedPropertyName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			
			if(bindedProperty == null) {
				Debug.LogError($"Property not found for {bindedPropertyName}!");
				return;
			}
			
			//check if their types are the same
			if(bindedProperty.PropertyType != typeof(TargetType)) {
				Debug.LogError($"Property type not match for {bindedProperty}");
				return;
			}
			
			UpdateBinding<TargetType,dynamic>(dataProperty, bindedProperty, getter, callback);
		}
	}

	class AbstractBasicEntityViewControllerImpl<T> : AbstractBasicEntityViewController<T> where T : class, IHaveCustomProperties, IHaveTags
	{
		protected override bool CanAutoRemoveEntityWhenLevelEnd { get; }
		protected override IEntity OnBuildNewEntity()
		{
			throw new NotImplementedException();
		}

		protected override void OnEntityStart()
		{
			throw new NotImplementedException();
		}

		protected override void OnBindEntityProperty()
		{
			throw new NotImplementedException();
		}
	}
}