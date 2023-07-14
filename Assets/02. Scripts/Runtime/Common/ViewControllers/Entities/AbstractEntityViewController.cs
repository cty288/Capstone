using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using UnityEngine;

namespace _02._Scripts.Runtime.Common.ViewControllers.Entities {
	public abstract class AbstractEntityViewController<T> : AbstractMikroController<MainGame>, IEntityViewController 
		where T : class, IEntity, new() {
		
		[field: ES3Serializable]
		[field: SerializeField]
		public string ID { get; private set; }
		IEntity IEntityViewController.Entity => BindedEntity;
	
		protected IEntityModel entityModel;
		protected T BindedEntity { get; private set; }
		
		private Dictionary<PropertyInfo, Func<object, object>> propertyBindings = new Dictionary<PropertyInfo, Func<object, object>>();

		
		public void Init(string id, IEntity entity) {
			ID = id;
			BindedEntity = entity as T;
		}
		
		protected virtual void Awake() {
			entityModel = this.GetModel<IEntityModel>();
		}

		protected virtual void Start() {
			if (string.IsNullOrEmpty(ID)) {
				Debug.LogError("ID for enemy is null or empty! Do not instantiate enemy view controller directly! " +
				               "Use EntityBuilderFactory instead!");
				return;
			}
			BindedEntity = entityModel.GetEntity<T>(ID);
			OnBindEntityProperty();
		}

		#region Property Binding

		protected abstract void OnBindEntityProperty();
		
		/// <summary>
		/// Automatically bind propertyName property to the real value of IPropertyType of th entity
		/// </summary>
		/// <param name="propertyName"></param>
		/// <typeparam name="IPropertyType"></typeparam>
		protected void Bind<IPropertyType>(string propertyName) where IPropertyType: class, IPropertyBase {
			var property = BindedEntity.GetProperty<IPropertyType>();
			if(property == null) {
				Debug.LogError("Property not found");
				return;
			}
			IBindableProperty bindableProperty = property.GetRealValue();
			Bind(propertyName, bindableProperty, property => property);
		}
		
		/// <summary>
		/// Automatically bind a BindableProperty to a property, the generic type of BindableProperty must be the same as the property
		/// </summary>
		/// <param name="bindedPropertyName"></param>
		/// <param name="bindableProperty"></param>
		/// <typeparam name="T">Type of the property, as well as the generic type of your bindable property</typeparam>
		protected void Bind<T>(string bindedPropertyName, BindableProperty<T> bindableProperty) {
			Bind(bindedPropertyName, bindableProperty, property => property );
		}
		
		
		/// <summary>
		/// Bind a BindableProperty to a property, with custom getter in case the property is not the same type as the BindableProperty
		/// </summary>
		/// <param name="bindedPropertyName"></param>
		/// <param name="bindableProperty"></param>
		/// <param name="getter">your custom getter, should return something with the same type as the target</param>
		/// <typeparam name="T">type of the target property</typeparam>
		/// <typeparam name="BindablePropertyType"></typeparam>
		protected void Bind<T, BindablePropertyType>(string bindedPropertyName, BindableProperty<BindablePropertyType> bindableProperty, 
			Func<BindablePropertyType, T> getter) {

			PropertyInfo bindedProperty = GetType().GetProperty(bindedPropertyName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			
			if(bindableProperty == null || bindedProperty == null) {
				Debug.LogError($"Property not found for {bindedProperty}!");
				return;
			}
			
			//check if their types are the same
			if(bindedProperty.PropertyType != typeof(T)) {
				Debug.LogError($"Property type not match for {bindedProperty}");
				return;
			}

			UpdateBinding(bindableProperty, bindedProperty, getter);
		}
		
		private void Bind<T>(string bindedPropertyName, IBindableProperty bindableProperty, Func<object, T> getter) {
			PropertyInfo bindedProperty = GetType().GetProperty(bindedPropertyName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			
			if(bindableProperty == null || bindedProperty == null) {
				Debug.LogError($"Property not found for {bindedProperty}!");
				return;
			}
			
			UpdateBinding(bindableProperty, bindedProperty, getter);
		}

		
		//for IBindableProperty
		private void UpdateBinding<T, BindablePropertyType>(IBindableProperty bindableProperty, 
			PropertyInfo bindedProperty, Func<BindablePropertyType, T> getter) {
			
			
			if(bindableProperty != null) {
				bindableProperty.RegisterWithInit(v => {
					if (this) {
						bindedProperty.SetValue(this, getter((BindablePropertyType) v), null);
					}
				});
				propertyBindings.Add(bindedProperty, property => getter((BindablePropertyType) bindableProperty.ObjectValue));

			}
		}
		
		//for BindableProperty<BindablePropertyType>
		private void UpdateBinding<T, BindablePropertyType>(BindableProperty<BindablePropertyType> bindableProperty, 
			PropertyInfo bindedProperty, Func<BindablePropertyType, T> getter) {
			
			
			if(bindableProperty != null) {
				bindableProperty.RegisterWithInitValue(v => {
					if (this) {
						bindedProperty.SetValue(this, getter(v));   
					}
				});
				propertyBindings.Add(bindedProperty, property => getter(bindableProperty.Value));

			}
		}

		protected void ForceUpdatePropertyBindings() {
			foreach (var propertyBinding in propertyBindings) {
				propertyBinding.Key.SetValue(this, propertyBinding.Value(propertyBinding.Key.GetValue(this)));
			}
		}
		
		#endregion
		
	}
}