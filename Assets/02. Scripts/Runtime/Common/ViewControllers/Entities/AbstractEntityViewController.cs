using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using _02._Scripts.Runtime.Common.ViewControllers.Entities.Enemies;
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
		
		private Dictionary<PropertyInfo, Func<object>> propertyBindings = new Dictionary<PropertyInfo, Func<object>>();
		private Dictionary<PropertyInfo, FieldInfo> propertyFields = new Dictionary<PropertyInfo, FieldInfo>();
		protected List<PropertyInfo> properties = new List<PropertyInfo>();

		private Type type;
		public void Init(string id, IEntity entity) {
			ID = id;
			BindedEntity = entity as T;
		}
		
		protected virtual void Awake() {
			entityModel = this.GetModel<IEntityModel>();
			type = typeof(T);
		}

		protected virtual void Start() {
			if (string.IsNullOrEmpty(ID)) {
				Debug.LogError("ID for enemy is null or empty! Do not instantiate enemy view controller directly! " +
				               "Use EntityBuilderFactory instead!");
				return;
			}
			BindedEntity = entityModel.GetEntity<T>(ID);
			OnBindProperty();
		}

		#region Property Binding

		protected void OnBindProperty() {
			CheckProperties();
			OnBindEntityProperty();
			BindPropertyAttributes();
		}

		private void CheckProperties() {
			PropertyInfo[] allProperties = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (PropertyInfo property in allProperties) {
				if (property.GetCustomAttributes(typeof(BindablePropertyAttribute), false).FirstOrDefault() is
				    BindablePropertyAttribute attribute) {
					if (property.CanWrite) {
						//the property must be read only
						Debug.LogError("Property " + property.Name + $" on {gameObject.name} is not read only! " +
						               "Please remove the set method of the property!");
					}
					else {
						properties.Add(property);
					}
				}
			}
		}


		protected abstract void OnBindEntityProperty();
		
		/// <summary>
		/// Automatically bind propertyName property to the real value of IPropertyType of th entity
		/// This is not recommended though it's convenient, because it will cost more performance
		/// </summary>
		/// <param name="propertyName"></param>
		/// <typeparam name="IPropertyType"></typeparam>
		protected void Bind<IPropertyType>(string propertyName, 
			Action<object, object> callback = null) where IPropertyType: class, IPropertyBase {
			var property = BindedEntity.GetProperty<IPropertyType>();
			if(property == null) {
				Debug.LogError("Property not found");
				return;
			}
			IBindableProperty bindableProperty = property.GetRealValue();
			Bind(propertyName, bindableProperty, property => property, callback);
		}
		
		/// <summary>
		/// Automatically bind a BindableProperty to a property, the generic type of BindableProperty must be the same as the property
		/// </summary>
		/// <param name="bindedPropertyName"></param>
		/// <param name="bindableProperty"></param>
		/// <typeparam name="T">Type of the property, as well as the generic type of your bindable property</typeparam>
		protected void Bind<T>(string bindedPropertyName, BindableProperty<T> bindableProperty, Action<T, T> callback = null) {
			Bind(bindedPropertyName, bindableProperty, property => property, callback );
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
			Func<BindablePropertyType, T> getter, Action<T, T> callback = null) {

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

			UpdateBinding(bindableProperty, bindedProperty, getter, callback);
		}
		
		private void Bind<T>(string bindedPropertyName, IBindableProperty bindableProperty, Func<object, T> getter,
			Action<T, T> callback) {
			PropertyInfo bindedProperty = GetType().GetProperty(bindedPropertyName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			
			if(bindableProperty == null || bindedProperty == null) {
				Debug.LogError($"Property not found for {bindedProperty}!");
				return;
			}
			
			UpdateBinding(bindableProperty, bindedProperty, getter, callback);
		}

		
		//for IBindableProperty
		private void UpdateBinding<T, BindablePropertyType>(IBindableProperty bindableProperty, 
			PropertyInfo bindedProperty, Func<BindablePropertyType, T> getter,  Action<T, T> callback) {
			
			
			if(bindableProperty != null) {
				Action<object, object> cb = null;
				cb = (oldValue, newValue) => {
					if (this) {
						SetReadOnlyProperty(bindedProperty, getter((BindablePropertyType) newValue));
						//bindedProperty.SetValue(this, getter((BindablePropertyType) newValue), null);
						callback?.Invoke(getter((BindablePropertyType) oldValue),
							getter((BindablePropertyType) newValue));
					}
					else {
						bindableProperty.UnRegisterOnObjectValueChanged(cb);
					}
				};
				bindableProperty.RegisterWithInitObject(cb).UnRegisterWhenGameObjectDestroyed(gameObject);
				propertyBindings.Add(bindedProperty, () => getter((BindablePropertyType) bindableProperty.ObjectValue));

			}
		}
		
		//for BindableProperty<BindablePropertyType>
		private void UpdateBinding<T, BindablePropertyType>(BindableProperty<BindablePropertyType> bindableProperty, 
			PropertyInfo bindedProperty, Func<BindablePropertyType, T> getter, Action<T, T> callback) {
			
			
			if(bindableProperty != null) {
				Action<BindablePropertyType, BindablePropertyType> cb = null;
				cb = (oldValue, newValue) => {
					if (this) {
						
						SetReadOnlyProperty(bindedProperty, getter(newValue));
						callback?.Invoke(getter(oldValue), getter(newValue));
					}
					else {
						bindableProperty.UnRegisterOnValueChanged(cb);
					}
				};
				bindableProperty.RegisterWithInitValue(cb).UnRegisterWhenGameObjectDestroyed(gameObject);
				propertyBindings.Add(bindedProperty, () => getter(bindableProperty.Value));

			}
		}

		protected void ForceUpdatePropertyBindings() {
			foreach (var propertyBinding in propertyBindings) {
				//propertyBinding.Key.SetValue(this, propertyBinding.Value(propertyBinding.Key.GetValue(this)));
				SetReadOnlyProperty(propertyBinding.Key,
					propertyBinding.Value());
			}
		}
		
		private void BindPropertyAttributes() {
			foreach (var prop in properties) {
				if (prop.GetCustomAttributes(typeof(BindablePropertyAttribute), false).FirstOrDefault() is BindablePropertyAttribute attribute) {
					
					IBindableProperty bindedProperty = BindedEntity.GetProperty(attribute.PropertyName).GetRealValue();
					
					//get type of the property
					Type propertyType = prop.PropertyType;
					Action<object,object> onChangedAction = null;
					if (attribute.OnChanged != null) {
						var method = GetType().GetMethod(attribute.OnChanged,
							BindingFlags.NonPublic | BindingFlags.Instance);
						
						Delegate func = Delegate.CreateDelegate(typeof(Action<,>).MakeGenericType(propertyType, propertyType),
							this, method);
						onChangedAction = (object oldValue, object newValue) => func.DynamicInvoke(oldValue, newValue);

					}
					
					if (attribute.GetterMethodName != null) 
					{
						//Bind(prop.Name, bindedProperty, attribute.GetterMethodName);
						
						var method = GetType().GetMethod(attribute.GetterMethodName, BindingFlags.NonPublic | BindingFlags.Instance);
						if (method != null) {
							var func = (Func<object, object>)Delegate.CreateDelegate(typeof(Func<object, object>), this, method);
							//func return type should be the same as the property type
							//Delegate func =  Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(typeof(object), propertyType), this, method);
							//Func<object, object> func2 = (object obj) => func.DynamicInvoke(obj);

							Bind(prop.Name, bindedProperty, func, onChangedAction);
						}
						else 
						{
							Debug.LogError($"Getter method {attribute.GetterMethodName} not found!");
						}
					}
					else {
						Bind(prop.Name, bindedProperty, property => property, onChangedAction);
					}
				}
			}
		}

		private void SetReadOnlyProperty(PropertyInfo propertyInfo, object value) {
			if (propertyFields.TryGetValue(propertyInfo, out FieldInfo field)) {
				field.SetValue(this, value);
			}
			else {
				string backingFieldName = $"<{propertyInfo.Name}>k__BackingField";
				Type type = propertyInfo.DeclaringType;
				FieldInfo fieldInfo = type.GetField(backingFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
				if (fieldInfo != null) {
					fieldInfo.SetValue(this, value);
					propertyFields.Add(propertyInfo, fieldInfo);
				}
			}
		}

		#endregion


		protected virtual void OnDestroy() {
			propertyBindings.Clear();
			propertyFields.Clear();
		}
	}
}