using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework;
using Mikrocosmos;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using MikroFramework.TimeSystem;
using MikroFramework.Utilities;
using Polyglot;
using Runtime.Controls;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.GameResources.ViewControllers;
using Runtime.Player;
using Runtime.UI.NameTags;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.DataFramework.ViewControllers.Entities {
	public abstract class AbstractEntityViewController<T> : DefaultPoolableGameObjectSaved, IEntityViewController 
		where T : class, IEntity {
		
		[field: ES3Serializable]
		public string ID { get; set; }
		[Header("Auto Create New Entity by OnBuildNewEntity() When Start")]
		[Tooltip("If not, you must manually call InitWithID() to initialize the entity.")]
		[SerializeField] protected bool autoCreateNewEntityWhenStart = false;

		[Header("Entity Name Tag")]
		[SerializeField] protected bool showNameTagWhenPointed = true;
		[SerializeField] protected Transform nameTagFollowTransform;
		[SerializeField] protected string nameTagPrefabName = "NameTag_General";
	
		[FormerlySerializedAs("triggerCheck")]
		[Header("Entity Interaction")]
		[SerializeField] protected bool hasInteractiveHint = false;
		[SerializeField] protected string interactiveHintPrefabName = "InteractHint_General";
		[SerializeField] protected string interactiveHintLocalizedKey = "interact";
		//[SerializeField] protected Transform hintCanvasFollowTransform = this.transform;
		
		[FormerlySerializedAs("autoRemoveEntityWhenDestroyed")]
		[Header("Entity Recycle Logic")]
		//[SerializeField, ES3Serializable] protected bool autoRemoveEntityWhenDestroyedOrRecycled = false;
		[SerializeField, ES3Serializable] protected bool autoDestroyWhenEntityRemoved = true;
		

		IEntity IEntityViewController.Entity => BoundEntity;


		protected IEntityModel entityModel;
		
		protected T BoundEntity { get; private set; }
		
		private Dictionary<PropertyInfo, Func<dynamic>> propertyBindings = new Dictionary<PropertyInfo, Func<dynamic>>();
		private Dictionary<PropertyInfo, FieldInfo> propertyFields = new Dictionary<PropertyInfo, FieldInfo>();
		protected List<PropertyInfo> properties = new List<PropertyInfo>();
		protected bool isPointed = false;
		protected Camera mainCamera;
		
		private class CrossHairManagedHUDInfo {
			public Transform originalSpawnTransform;
			public KeepGlobalRotation originalSpawnPositionOffset;
			public KeepGlobalRotation realSpawnPositionOffset;
			public GameObject spawnedHUD;
			public HUDCategory hudCategory;
			public Vector3 targetPos;
			
			public CrossHairManagedHUDInfo(Transform originalSpawnTransform, KeepGlobalRotation originalSpawnPositionOffset,
				KeepGlobalRotation realSpawnPositionOffset, GameObject spawnedHUD, HUDCategory hudCategory) {
				this.originalSpawnTransform = originalSpawnTransform;
				this.originalSpawnPositionOffset = originalSpawnPositionOffset;
				this.realSpawnPositionOffset = realSpawnPositionOffset;
				this.spawnedHUD = spawnedHUD;
				this.hudCategory = hudCategory;
				this.targetPos = this.originalSpawnPositionOffset.PositionOffset;
			}
		}


		private Dictionary<string, CrossHairManagedHUDInfo> crossHairManagedHUDs =
			new Dictionary<string, CrossHairManagedHUDInfo>();
		
		private LayerMask crossHairHUDManagedDetectLayerMask;

		protected override void Awake() {
			base.Awake();
			crossHairHUDManagedDetectLayerMask = LayerMask.GetMask("CrossHairDetect");
			//get all layers except for the crosshair detect layer
			crossHairHUDManagedDetectLayerMask = ~crossHairHUDManagedDetectLayerMask;
			CheckProperties();
			mainCamera = Camera.main;
		}

		

		public override void OnStartOrAllocate() {
			base.OnStartOrAllocate();
			OnPlayerInteractiveZoneNotReachable(null, null);
			
			OnStart();
			
		}

		public void InitWithID(string id) {
			ID = id;
			IEntity ent = null;
			(ent, entityModel) = GlobalEntities.GetEntityAndModel(ID);
			if (ent == null) {
				Debug.LogError("Entity with ID " + ID + " not found");
				return;
			}

			if (BoundEntity != null) {
				BoundEntity.UnRegisterOnEntityRecycled(OnEntityRecycled);
			}
			BoundEntity = ent as T;
			BoundEntity.RegisterOnEntityRecycled(OnEntityRecycled).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			OnBindProperty();
			OnEntityStart();
		}

		public virtual void OnPointByCrosshair() {
			isPointed = true;
			if (showNameTagWhenPointed) {
				if(nameTagFollowTransform) {
					GameObject
						nameTag = SpawnCrosshairResponseHUDElement(nameTagFollowTransform, nameTagPrefabName, HUDCategory.NameTag);
					
					if (nameTag) {
						INameTag nameTagComponent = nameTag.GetComponent<INameTag>();
						if (nameTagComponent != null) {
							nameTagComponent.SetName(BoundEntity.GetDisplayName());
						}
					}
				}
			}

			foreach (AbstractEntityViewController<T>.CrossHairManagedHUDInfo hudInfo in crossHairManagedHUDs.Values) {
				hudInfo.spawnedHUD.SetActive(true);
			}
		}

		public virtual void OnUnPointByCrosshair() {
			isPointed = false;
			
			foreach (AbstractEntityViewController<T>.CrossHairManagedHUDInfo hudInfo in crossHairManagedHUDs.Values) {
				hudInfo.spawnedHUD.SetActive(false);
			}
			
			if (showNameTagWhenPointed && nameTagFollowTransform) {
				DespawnHUDElement(nameTagFollowTransform, HUDCategory.NameTag);
			}
			
			
		}
		
		/// <summary>
		/// Spawb a UI HUD element that automatically follows the transform when pointed by crosshair
		/// It will automatically adjust its position if it blocks the view
		/// </summary>
		/// <param name="followTransform"></param>
		/// <param name="prefabName"></param>
		/// <param name="category"></param>
		/// <returns></returns>
		protected GameObject SpawnCrosshairResponseHUDElement(Transform followTransform, string prefabName, HUDCategory category) {
			if(followTransform.TryGetComponent<KeepGlobalRotation>(out var keepGlobalRotation)) {
				
				
				KeepGlobalRotation realHealthBarPositionOffset = Instantiate(followTransform, followTransform.parent)
					.GetComponent<KeepGlobalRotation>();
				
				GameObject spawnedElement = HUDManager.Singleton.SpawnHUDElement(realHealthBarPositionOffset.transform, prefabName, category, true);
				spawnedElement.transform.localScale = followTransform.localScale;
				spawnedElement.SetActive(isPointed);


				string id = followTransform.GetHashCode().ToString() + category.ToString();
				
				crossHairManagedHUDs.Add(id,
					new CrossHairManagedHUDInfo(followTransform, keepGlobalRotation, realHealthBarPositionOffset,
						spawnedElement, category));
				
				return spawnedElement;
			}else {
				throw new Exception("The transform must have a KeepGlobalRotation component");
			}
		}
		
		protected void DespawnHUDElement(Transform followTransform, HUDCategory category) {
			string id = followTransform.GetHashCode().ToString() + category.ToString();
			if (crossHairManagedHUDs.TryGetValue(id, out var info)) {
				HUDManager.Singleton.DespawnHUDElement(info.realSpawnPositionOffset.transform, category);
				Destroy(info.realSpawnPositionOffset);
				crossHairManagedHUDs.Remove(id);
			}
			
		}

		protected virtual void FixedUpdate() {
			if (isPointed) {
				var camTr = mainCamera.transform;
				foreach (AbstractEntityViewController<T>.CrossHairManagedHUDInfo hudInfo in crossHairManagedHUDs.Values) {
					if (!hudInfo.spawnedHUD) {
						continue;
					}
					RaycastHit hit;
					bool hitSelf = false;
					
					if (Physics.Raycast(camTr.position, hudInfo.originalSpawnTransform.transform.position - camTr.position, out hit, 100f, crossHairHUDManagedDetectLayerMask)) {
						if (!hit.collider.isTrigger && hit.collider.attachedRigidbody && hit.collider.attachedRigidbody.gameObject == gameObject) {
							hitSelf = true;

							Transform realHealthBarSpawnPoint = hudInfo.realSpawnPositionOffset.transform;
							if (Physics.Raycast(camTr.position, realHealthBarSpawnPoint.position - camTr.position, out hit, 100f, crossHairHUDManagedDetectLayerMask)) {
								if (hit.collider.attachedRigidbody && hit.collider.attachedRigidbody.gameObject == gameObject) {
									hudInfo.targetPos += Vector3.up * 0.5f;
									//if the player is right below (or very close in terms of x and z) the enemy, we need to also move the health bar in both x and z axis until it doesn't hit the enemy
									if (Math.Abs(camTr.position.x - realHealthBarSpawnPoint.position.x) < 10f &&
									    Math.Abs(camTr.position.z - realHealthBarSpawnPoint.position.z) < 10f) {
										hudInfo.targetPos += new Vector3(0.5f, 0.5f, 0);
									}
									
								}
							}
						}
					}
					
					
					
					if (!hitSelf) {
						hudInfo.targetPos = hudInfo.originalSpawnPositionOffset.PositionOffset;
					}

					hudInfo.realSpawnPositionOffset.PositionOffset =
						Vector3.Lerp(hudInfo.realSpawnPositionOffset.PositionOffset, hudInfo.targetPos, 0.1f);
				}
			}
		}


		protected virtual void OnEntityRecycled(IEntity ent) {
			if (autoDestroyWhenEntityRemoved) {
				RecycleToCache();
			}
			
		}
		

		protected virtual void OnStart() {
			string id = ID;
			if (string.IsNullOrEmpty(ID)) {
				if (autoCreateNewEntityWhenStart) {
					IEntity entity = OnBuildNewEntity();
					if (entity != null) {
						id = entity.UUID;
						InitWithID(id);
					}
				}
			}
			else { //load from saved
				InitWithID(id);
			}
		}

		public override void OnRecycled() {
			base.OnRecycled();
			if (BoundEntity != null) {
				BoundEntity.UnRegisterOnEntityRecycled(OnEntityRecycled);
			}
			/*if (autoRemoveEntityWhenDestroyedOrRecycled) {
				entityModel.RemoveEntity(ID);
			}*/
			ID = null;
			BoundEntity = null;
			propertyBindings.Clear();

			string[] keys = crossHairManagedHUDs.Keys.ToArray();
			foreach (string key in keys) {
				DespawnHUDElement(crossHairManagedHUDs[key].originalSpawnTransform,
					crossHairManagedHUDs[key].hudCategory);
			}
			crossHairManagedHUDs.Clear();
			isPointed = false;

		}

		protected abstract IEntity OnBuildNewEntity();

		#region Property Binding

		protected void OnBindProperty() {
			
			OnBindEntityProperty();
			BindPropertyAttributes();
			
		}

		protected abstract void OnEntityStart();

		private void CheckProperties() {
			PropertyInfo[] allProperties = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (PropertyInfo property in allProperties) {
				if (property.GetCustomAttributes(typeof(BindAttribute), false).FirstOrDefault() is BindAttribute attribute) {
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
				bindableProperty.RegisterWithInitObject(cb).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
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
				bindableProperty.RegisterWithInitValue(cb).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
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

		
		public virtual void OnPlayerInteractiveZoneReachable(GameObject player, PlayerInteractiveZone zone) {
			if (hasInteractiveHint) {
				GameObject hud = HUDManager.Singleton.SpawnHUDElement(transform, interactiveHintPrefabName,
					HUDCategory.InteractiveTag, true);
				if (hud) {
					InteractiveHint element = hud.GetComponent<InteractiveHint>();
					if (element != null) {
						element.SetHint(ClientInput.Singleton.FindActionInPlayerActionMap("Interact"),
							Localization.Get(interactiveHintLocalizedKey));
					}
				}
			}
			
		}
		

		public virtual void OnPlayerInteractiveZoneNotReachable(GameObject player, PlayerInteractiveZone zone) {
			if (hasInteractiveHint) {
				HUDManager.Singleton.DespawnHUDElement(transform, HUDCategory.InteractiveTag);
			}
		}

		public virtual void OnPlayerInInteractiveZone(GameObject player, PlayerInteractiveZone zone) {
			
		}

		public virtual void OnPlayerExitInteractiveZone(GameObject player, PlayerInteractiveZone zone) {
			
		}


		protected virtual void OnDestroy() {
			propertyBindings.Clear();
			propertyFields.Clear();
			/*if (autoRemoveEntityWhenDestroyedOrRecycled && entityModel!=null) {
				entityModel.RemoveEntity(ID);
			}*/
		}
	}
}