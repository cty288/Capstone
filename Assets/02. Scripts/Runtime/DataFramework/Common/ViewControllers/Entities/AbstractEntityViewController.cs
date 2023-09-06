using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework;
using Mikrocosmos.Controls;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using MikroFramework.TimeSystem;
using MikroFramework.Utilities;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.DataFramework.ViewControllers.Entities {
	public abstract class AbstractEntityViewController<T> : AbstractMikroController<MainGame>, IEntityViewController 
		where T : class, IEntity, new() {

		
		
		[field: ES3Serializable]
		//[field: SerializeField]
		public string ID { get; set; }

		[Header("Entity Name Tag")]
		[SerializeField] protected bool showNameTagWhenPointed = true;
		[SerializeField] protected Transform nameTagFollowTransform;
		[SerializeField] protected string nameTagPrefabName = "NameTag_General";
	
		[FormerlySerializedAs("triggerCheck")]
		[Header("Entity Interaction")]
		[SerializeField] protected TriggerCheck interactiveHintTriggerCheck;
		[SerializeField] protected string interactiveHintPrefabName = "InteractHint_General";
		[SerializeField] protected string interactiveHintLocalizedKey = "interact";
		//[SerializeField] protected Transform hintCanvasFollowTransform = this.transform;
		
		[Header("Entity Recycle Logic")]
		[SerializeField, ES3Serializable] protected bool autoRemoveEntityWhenDestroyed = false;
		[SerializeField, ES3Serializable] protected bool autoDestroyWhenEntityRemoved = true;
		

		IEntity IEntityViewController.Entity => BoundEntity;


		private IEntityModel entityModel;
		
		protected T BoundEntity { get; private set; }
		
		private Dictionary<PropertyInfo, Func<dynamic>> propertyBindings = new Dictionary<PropertyInfo, Func<dynamic>>();
		private Dictionary<PropertyInfo, FieldInfo> propertyFields = new Dictionary<PropertyInfo, FieldInfo>();
		protected List<PropertyInfo> properties = new List<PropertyInfo>();
		
		
		protected virtual void Awake() {
			interactiveHintTriggerCheck = GetComponent<TriggerCheck>();
			if (interactiveHintTriggerCheck) {
				interactiveHintTriggerCheck.OnEnter += OnEnterInteractiveCheck;
				interactiveHintTriggerCheck.OnExit += OnExitInteractiveCheck;
				TryHideHint();
			}

		}


		protected virtual void Start() {
			OnStart();
			OnEntityStart();
		}

		public void InitWithID(string id) {
			ID = id;
			IEntity ent = null;
			(ent, entityModel) = GlobalEntities.GetEntityAndModel(ID);
			if (ent == null) {
				Debug.LogError("Entity with ID " + ID + " not found");
				return;
			}
			BoundEntity = ent as T;
			BoundEntity.RegisterOnEntityRecycled(OnEntityRecycled).UnRegisterWhenGameObjectDestroyed(gameObject);
		}

		public void OnPointByCrosshair() {
			if (showNameTagWhenPointed) {
				if(!nameTagFollowTransform) {
					Debug.LogError($"Name tag follow transform not set for {gameObject.name}!");
					return;
				}

				GameObject nameTag = HUDManager.Singleton.SpawnHUDElement(nameTagFollowTransform, nameTagPrefabName, HUDCategory.NameTag);
				if (nameTag) {
					INameTag nameTagComponent = nameTag.GetComponent<INameTag>();
					if (nameTagComponent != null) {
						nameTagComponent.SetName(BoundEntity.GetDisplayName());
					}
				}
			}
		}

		public void OnUnPointByCrosshair() {
			if (showNameTagWhenPointed && nameTagFollowTransform) {
				HUDManager.Singleton.DespawnHUDElement(nameTagFollowTransform, HUDCategory.NameTag);
			}
			
		}

		protected virtual void OnEntityRecycled(IEntity ent) {
			if (autoDestroyWhenEntityRemoved) {
				Destroy(gameObject);
			}
		}

		protected virtual void OnStart() {
			string id = ID;
			if (string.IsNullOrEmpty(ID)) {
				IEntity entity = OnBuildNewEntity();
				id = entity.UUID;
			}
			InitWithID(id);
			OnBindProperty();
		}

		protected abstract IEntity OnBuildNewEntity();

		#region Property Binding

		protected void OnBindProperty() {
			CheckProperties();
			OnBindEntityProperty();
			BindPropertyAttributes();
			
		}

		protected abstract void OnEntityStart();

		private void CheckProperties() {
			PropertyInfo[] allProperties = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (PropertyInfo property in allProperties) {
				if (property.GetCustomAttributes(typeof(BindAttribute), false).FirstOrDefault() is
				    BindAttribute attribute) {
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
		/// Automatically bind propertyName property to the real value of IPropertyType of th entity. This will not bind to nested properties
		/// This is not recommended though it's convenient, because it will cost more performance
		/// </summary>
		/// <param name="propertyName"></param>
		/// <typeparam name="IPropertyType"></typeparam>
		protected void Bind<IPropertyType>(string bindedPropertyName, 
			Action<dynamic, dynamic> callback = null) where IPropertyType: class, IPropertyBase {
			var property = BoundEntity.GetProperty<IPropertyType>();
			if(property == null) {
				Debug.LogError("Property not found");
				return;
			}
			IBindableProperty bindableProperty = property.GetRealValue();
			Bind(bindedPropertyName, bindableProperty, property => property, callback);
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
		/// <typeparam name="TargetType">type of the target property</typeparam>
		/// <typeparam name="BindedDataType">Type of the value of the source property</typeparam>
		protected void Bind<BindedDataType, TargetType>(string bindedPropertyName, BindableProperty<BindedDataType> bindableProperty, 
			Func<BindedDataType, TargetType> getter, Action<TargetType, TargetType> callback = null) {

			PropertyInfo bindedProperty = GetType().GetProperty(bindedPropertyName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			
			if(bindableProperty == null || bindedProperty == null) {
				Debug.LogError($"Property not found for {bindedProperty}!");
				return;
			}
			
			//check if their types are the same
			if(bindedProperty.PropertyType != typeof(TargetType)) {
				Debug.LogError($"Property type not match for {bindedProperty}");
				return;
			}

			UpdateBinding(bindableProperty, bindedProperty, getter, callback);
		}
		
		protected void Bind<T>(string bindedPropertyName, IBindableProperty bindableProperty, Func<dynamic, T> getter,
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
		protected void UpdateBinding<T, BindablePropertyType>(IBindableProperty bindableProperty, 
			PropertyInfo bindedProperty, Func<BindablePropertyType, T> getter,  Action<T, T> callback) {
			
			
			if(bindableProperty != null) {
				Action<dynamic, dynamic> cb = null;
				cb = (oldValue, newValue) => {
					if (this) {
						SetReadOnlyProperty(bindedProperty, getter(newValue));
						//bindedProperty.SetValue(this, getter((BindablePropertyType) newValue), null);
						callback?.Invoke(getter(oldValue),
							getter(newValue));
					}
					else {
						bindableProperty.UnRegisterOnObjectValueChanged(cb);
					}
				};
				bindableProperty.RegisterWithInitObject(cb).UnRegisterWhenGameObjectDestroyed(gameObject);
				propertyBindings.Add(bindedProperty, () => getter((BindablePropertyType) bindableProperty.Value));

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
				if (prop.GetCustomAttributes(typeof(BindAttribute), false).FirstOrDefault() is BindAttribute attribute) {
					
					IBindableProperty bindedProperty = BoundEntity.GetProperty(attribute.PropertyName).GetRealValue();
					
					//get type of the property
					Type propertyType = prop.PropertyType;
					Action<dynamic,dynamic> onChangedAction = null;
					if (attribute.OnChanged != null) {
						var method = GetType().GetMethod(attribute.OnChanged,
							BindingFlags.NonPublic | BindingFlags.Instance);
						
						Delegate func = Delegate.CreateDelegate(typeof(Action<,>).MakeGenericType(propertyType, propertyType),
							this, method);
						onChangedAction = ( oldValue,  newValue) => func.DynamicInvoke(oldValue, newValue);

					}
					
					if (attribute.GetterMethodName != null) 
					{
						//Bind(prop.Name, bindedProperty, attribute.GetterMethodName);
						
						var method = GetType().GetMethod(attribute.GetterMethodName, BindingFlags.NonPublic | BindingFlags.Instance);
						if (method != null) {
							var func = (Func<dynamic, dynamic>)Delegate.CreateDelegate(typeof(Func<dynamic, dynamic>), this, method);
							//func return type should be the same as the property type
							//var func = (Func<dynamic, dynamic>) Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(bindedProperty.Value.GetType(), method.ReturnType), this, method);
							

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

		private void SetReadOnlyProperty(PropertyInfo propertyInfo, dynamic value) {
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

		
		private void OnEnterInteractiveCheck(Collider other) {
			if(other.CompareTag("Player")) {
				TryShowHint();
			}
		}
		private void OnExitInteractiveCheck(Collider other) {
			if(other.CompareTag("Player")) {
				TryHideHint();
            
			}
		}
		
		protected virtual void TryShowHint() {
			GameObject hud = HUDManager.Singleton.SpawnHUDElement(transform, interactiveHintPrefabName, HUDCategory.InteractiveTag);
			if (hud) {
				InteractiveHint element = hud.GetComponent<InteractiveHint>();
				if (element != null) {
					element.SetHint(ClientInput.Singleton.FindActionInPlayerActionMap("Interact"),
						Localization.Get(interactiveHintLocalizedKey));
				}
			}
		}
    
		protected virtual void TryHideHint() {
			if (interactiveHintTriggerCheck) {
				HUDManager.Singleton.DespawnHUDElement(interactiveHintTriggerCheck.transform, HUDCategory.InteractiveTag);
			}
		}


		protected virtual void OnDestroy() {
			propertyBindings.Clear();
			propertyFields.Clear();
			if (autoRemoveEntityWhenDestroyed) {
				entityModel.RemoveEntity(ID);
			}

			if (interactiveHintTriggerCheck) {
				interactiveHintTriggerCheck.OnEnter -= OnEnterInteractiveCheck;
				interactiveHintTriggerCheck.OnExit -= OnExitInteractiveCheck;
			}
		}
	}
}