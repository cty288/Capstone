using System;
using System.Reflection;
using _02._Scripts.Runtime.Base.Entity.ClassifiedEntity;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using UnityEngine;

namespace _02._Scripts.Runtime.Common.ViewControllers.Entities {
	public abstract class AbstractHaveCustomPropertyEntityViewController<T, TEntityModel>: AbstractEntityViewController<T, TEntityModel> 
		where T : class, IHaveCustomProperties, new()
		where TEntityModel: class, IEntityModel
	{
		
		protected void BindCustomData<T>(string bindedPropertyName, string customPropertyName, string customDataName,
			Action<T, T?> callback = null) {
			IBindableProperty dataProperty = BindedEntity.GetCustomDataValue(customPropertyName, customDataName);
			if(dataProperty == null) {
				throw new Exception(
					$"Custom data property {customDataName} doesn't exist in custom property {customPropertyName}");
			}

			Bind<T>(bindedPropertyName, dataProperty, property => property, callback);
		}


		protected void BindCustomData<BindedDataType, TargetType>(string bindedPropertyName, string customPropertyName, string customDataName,
			Func<BindedDataType, TargetType> getter, Action<TargetType, TargetType> callback = null) {
			IBindableProperty dataProperty = BindedEntity.GetCustomDataValue(customPropertyName, customDataName);
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
			
			UpdateBinding(dataProperty, bindedProperty, getter, callback);
		}
	}
}