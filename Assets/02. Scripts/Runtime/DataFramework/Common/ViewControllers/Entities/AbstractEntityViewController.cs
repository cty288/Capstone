using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _02._Scripts.Runtime.Levels.Models;
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
using Runtime.Weapons.ViewControllers;
using Runtime.Weapons.ViewControllers.CrossHairs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Runtime.DataFramework.ViewControllers.Entities {

	public class EntityVCOnRecycledUnRegister : IUnRegister {
		private Action<IEntityViewController> onEntityRecycled;

		private IEntityViewController entity;

		public EntityVCOnRecycledUnRegister(IEntityViewController entity,
			Action<IEntityViewController> onEntityRecycled) {
			this.entity = entity;
			this.onEntityRecycled = onEntityRecycled;
		}

		public void UnRegister() {
			entity.UnRegisterOnEntityViewControllerInit(onEntityRecycled);
			entity = null;
		}
	}

	

	public abstract class AbstractEntityViewController<T> : DefaultPoolableGameObjectSaved, IEntityViewController, ICrossHairDetectable
		where T : class, IEntity {
		
		[field: ES3Serializable]
		public string ID { get; set; }

		[field: SerializeField]
		public string PrefabID { get; set; }

		[Header("Auto Create New Entity by OnBuildNewEntity() When Start")]
		[Tooltip("If not, you must manually call InitWithID() to initialize the entity.")]
		[SerializeField] protected bool autoCreateNewEntityWhenStart = false;

		[Header("Entity Name Tag")]
		[SerializeField] protected bool showNameTagWhenPointed = true;
		[SerializeField] protected Transform nameTagFollowTransform;
		[SerializeField] protected string nameTagPrefabName = "NameTag_General";
		[SerializeField] protected bool nameTagAutoAdjustHeight = true;
	
		[FormerlySerializedAs("triggerCheck")]
		[Header("Entity Interaction")]
		[SerializeField] protected bool hasInteractiveHint = false;
		[SerializeField] protected string interactiveHintPrefabName = "InteractHint_General";
		[SerializeField] protected string interactiveHintLocalizedKey = "interact";
		[SerializeField] 
		protected float interactiveHintToleranceMaxTime = 0.3f;
		protected float interactiveHintToleranceTimer = 0f;

		[SerializeField] protected Transform interactiveHintFollowTransform;

		[Tooltip("<=0 means no hold time required (press).")]
		[SerializeField] protected float interactiveHintHoldTime = -1;

		protected float interactiveHintHoldTimer = 0f;
		//[SerializeField] protected Transform hintCanvasFollowTransform = this.transform;
		
		[FormerlySerializedAs("autoRemoveEntityWhenDestroyed")]
		[Header("Entity Recycle Logic")]
		//[SerializeField, ES3Serializable] protected bool autoRemoveEntityWhenDestroyedOrRecycled = false;
		[SerializeField, ES3Serializable] protected bool autoDestroyWhenEntityRemoved = true;
		//[SerializeField, ES3Serializable] protected bool autoRemoveEntityWhenLevelEnd = false;
		protected abstract bool CanAutoRemoveEntityWhenLevelEnd { get; }
		
		[Header("HUD Related")]
		[Tooltip("This is the tolerance time for the cross hair HUD to disappear after the entity is not pointed.")] 
		[SerializeField] 
		protected float crossHairHUDToleranceMaxTime = 0.3f;

		[SerializeField] protected float crossHairHUDToleranceScreenDistanceFactor = 0.5f;
		
		protected float crossHairHUDTimer = 0f;
		private bool readyToRecycled = false;
		

		IEntity IEntityViewController.Entity => BoundEntity;


		protected IEntityModel entityModel;
		
		
		protected T BoundEntity { get; private set; }
		
		private Dictionary<PropertyInfo, Func<dynamic>> propertyBindings = new Dictionary<PropertyInfo, Func<dynamic>>();
		private Dictionary<PropertyInfo, FieldInfo> propertyFields = new Dictionary<PropertyInfo, FieldInfo>();
		protected List<PropertyInfo> properties = new List<PropertyInfo>();
		protected bool isPointed = false;
		protected Camera mainCamera;
		protected Action<IEntityViewController> onEntityVCInitCallback = null;
		//[SerializeField][ReadOnly] protected string prefabID = null;
		
		protected class CrossHairManagedHUDInfo {
			public Transform originalSpawnTransform;
			public KeepGlobalRotation originalSpawnPositionOffset;
			public KeepGlobalRotation realSpawnPositionOffset;
			public GameObject spawnedHUD;
			public HUDCategory hudCategory;
			public Vector3 targetPos;
			public bool AutoAdjust;
			
			public CrossHairManagedHUDInfo(Transform originalSpawnTransform, KeepGlobalRotation originalSpawnPositionOffset,
				KeepGlobalRotation realSpawnPositionOffset, GameObject spawnedHUD, HUDCategory hudCategory, bool autoAdjust) {
				this.originalSpawnTransform = originalSpawnTransform;
				this.originalSpawnPositionOffset = originalSpawnPositionOffset;
				this.realSpawnPositionOffset = realSpawnPositionOffset;
				this.spawnedHUD = spawnedHUD;
				this.hudCategory = hudCategory;
				this.targetPos = this.originalSpawnPositionOffset.PositionOffset;
				this.AutoAdjust = autoAdjust;
			}
		}


		private Dictionary<string, CrossHairManagedHUDInfo> crossHairManagedHUDs =
			new Dictionary<string, CrossHairManagedHUDInfo>();
		
		private LayerMask crossHairHUDManagedDetectLayerMask;


		private bool playerCanInteract => hasInteractiveHint && currentInteractiveHint;

		//protected bool interactable = true;
		//protected bool isInteracting;
		protected override void Awake() {
			base.Awake();
			crossHairHUDManagedDetectLayerMask = LayerMask.GetMask("CrossHairDetect");
			//get all layers except for the crosshair detect layer
			crossHairHUDManagedDetectLayerMask = ~crossHairHUDManagedDetectLayerMask;
			CheckProperties();
			mainCamera = Camera.main;
			canInteractRc.RegisterOnValueChanged(OnCanInteractChanged)
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			//Assert.IsTrue(this.enabled);
		}




		public override void OnStartOrAllocate() {
			base.OnStartOrAllocate();
			OnPlayerInteractiveZoneNotReachable(null, null);
			
			OnStart();
			
		}

		public void InitWithID(string id, bool recycleIfAlreadyExist = true) {
			if (BoundEntity != null && id == BoundEntity.UUID) {
				return;
			}
			
			IEntity ent = null;
			(ent, entityModel) = GlobalEntities.GetEntityAndModel(id);
			if (ent == null) {
				Debug.LogError("Entity with ID " + id + " not found");
				return;
			}

			//ILevelEntity levelEntity = this.GetModel<ILevelModel>().CurrentLevel.Value;
			ILevelModel levelModel = this.GetModel<ILevelModel>();
			
			
			
			bool needToRecallStart = false;
			if (BoundEntity != null) {
				BoundEntity.UnRegisterReadyToRecycle(OnEntityReadyToRecycle);
				BoundEntity.UnRegisterOnEntityRecycled(OnEntityRecycled);
				//levelModel.CurrentLevel.UnRegisterOnValueChanged(OnLevelChange);
				if (recycleIfAlreadyExist) {
					entityModel.RemoveEntity(BoundEntity.UUID);
				}
				
				OnReadyToRecycle();
				OnRecycled();
				needToRecallStart = true;
			}
			ID = id;
			BoundEntity = ent as T;
			BoundEntity.RegisterOnEntityRecycled(OnEntityRecycled).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			BoundEntity.RegisterReadyToRecycle(OnEntityReadyToRecycle);
			levelModel.CurrentLevel.RegisterOnValueChanged(OnLevelChange)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			
			OnBindProperty();
			OnEntityStart();
			onEntityVCInitCallback?.Invoke(this);
			if (needToRecallStart) {
				OnStart();
			}
		}

		private void OnLevelChange(ILevelEntity oldLevel, ILevelEntity newLevel) {
			if (CanAutoRemoveEntityWhenLevelEnd  && newLevel.UUID != oldLevel.UUID) {
				transform.SetParent(null);
				entityModel.RemoveEntity(BoundEntity.UUID);
			}
		}
		

		


		
		
		private IEnumerator ShowNameTagDelayed(GameObject nameTag, CrossHairManagedHUDInfo hudInfo) {
			AdjustHUD(hudInfo);
			yield return new WaitForEndOfFrame();
			yield return null;
			
			nameTag.SetActive(true);
		}



		private void OnHUDUnpointedTorlanceTimeEnds() {
			foreach (AbstractEntityViewController<T>.CrossHairManagedHUDInfo hudInfo in crossHairManagedHUDs.Values) {
				hudInfo.spawnedHUD.SetActive(false);
			}
			if (showNameTagWhenPointed && nameTagFollowTransform && !String.IsNullOrEmpty(nameTagPrefabName)) {
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
		protected (GameObject, CrossHairManagedHUDInfo) SpawnCrosshairResponseHUDElement(Transform followTransform, string prefabName, HUDCategory category, bool autoAdjust = true) {
			string id = followTransform.GetHashCode().ToString() + category.ToString();
			if (crossHairManagedHUDs.ContainsKey(id)) {
				return (crossHairManagedHUDs[id].spawnedHUD, crossHairManagedHUDs[id]);
			}
			
			if(followTransform.TryGetComponent<KeepGlobalRotation>(out var keepGlobalRotation)) {
				
				
				KeepGlobalRotation realHealthBarPositionOffset = Instantiate(followTransform, followTransform.parent)
					.GetComponent<KeepGlobalRotation>();
				
				GameObject spawnedElement = HUDManager.Singleton.SpawnHUDElement(realHealthBarPositionOffset.transform, prefabName, category, true);
				spawnedElement.transform.localScale = followTransform.localScale;
				spawnedElement.SetActive(isPointed);


				
				
				crossHairManagedHUDs.TryAdd(id,
					new CrossHairManagedHUDInfo(followTransform, keepGlobalRotation, realHealthBarPositionOffset,
						spawnedElement, category, autoAdjust));

				return (spawnedElement, crossHairManagedHUDs[id]);
			}else {
				throw new Exception("The transform must have a KeepGlobalRotation component");
			}
		}
		
		protected void DespawnHUDElement(Transform followTransform, HUDCategory category) {
			string id = followTransform.GetHashCode().ToString() + category.ToString();
			if (crossHairManagedHUDs.TryGetValue(id, out var info)) {
				HUDManager.Singleton.DespawnHUDElement(info.realSpawnPositionOffset.transform, category);
				Destroy(info.realSpawnPositionOffset.gameObject);
				crossHairManagedHUDs.Remove(id);
			}
			
		}

		private bool unPointTriggered = false;
		private bool unInteractTriggered = false;

		protected virtual void Update() {
			if (playerCanInteract) {
				if (interactiveHintHoldTime <= 0) {
					if (ClientInput.Singleton.GetSharedActions().Interact.WasPressedThisFrame()) {
						OnPlayerPressInteract();
					}
				}
				else {
					if (ClientInput.Singleton.GetSharedActions().Interact.IsPressed()) {
						interactiveHintHoldTimer += Time.deltaTime;
						float progress = interactiveHintHoldTimer / interactiveHintHoldTime;
						currentInteractiveHint.SetFiller(progress);
						if (interactiveHintHoldTimer >= interactiveHintHoldTime) {
							OnPlayerPressInteract();
						}
					}
					else {
						interactiveHintHoldTimer = 0;
						currentInteractiveHint.SetFiller(0);
					}
				}
			}
			
			if (crossHairHUDTimer < crossHairHUDToleranceMaxTime && !isPointed && !unPointTriggered) {
				crossHairHUDTimer += Time.deltaTime;
				//if the screen distance between crosshair and this game obj is too far, then directly set timer to tolerance time

				Vector3 crossHairScreenPos = Crosshair.Singleton.CrossHairScreenPosition;
				Vector3 thisScreenPos = mainCamera.WorldToScreenPoint(transform.position);
				float distance = Vector3.Distance(crossHairScreenPos, thisScreenPos);
				if (distance >= crossHairHUDToleranceScreenDistanceFactor * Screen.width) {
					crossHairHUDTimer = crossHairHUDToleranceMaxTime;
				}
				
				if (crossHairHUDTimer >= crossHairHUDToleranceMaxTime) {
					crossHairHUDTimer = 0;
					unPointTriggered = true;
					OnHUDUnpointedTorlanceTimeEnds();
				}
			}

			if (interactiveHintToleranceTimer < interactiveHintToleranceMaxTime && !isInteractiveHintActive && !unInteractTriggered) {
				interactiveHintToleranceTimer += Time.deltaTime;
				
				if (interactiveHintToleranceTimer >= interactiveHintToleranceMaxTime) {
					interactiveHintToleranceTimer = 0;
					unInteractTriggered = true;
					
					Transform tr = interactiveHintFollowTransform ? interactiveHintFollowTransform : transform;
					HUDManager.Singleton.DespawnHUDElement(tr, HUDCategory.InteractiveTag);
					currentInteractiveHint = null;
					interactiveHintHoldTimer = 0f;
				}
			}
			
			
			foreach (AbstractEntityViewController<T>.CrossHairManagedHUDInfo hudInfo in crossHairManagedHUDs.Values) {
				if (!hudInfo.AutoAdjust) {
					continue;
				}
				AdjustHUD(hudInfo);
			}
		}

		protected virtual void OnPlayerPressInteract() {
			
		}


		private void AdjustHUD(CrossHairManagedHUDInfo hudInfo) {
			var camTr = mainCamera.transform;
			if (!hudInfo.spawnedHUD) {
				return;
			}



			RaycastHit hit;
			bool needAdjust = false;
			if (hudInfo.AutoAdjust) {
				//get the screen pos of this game obj and original target. If they are too close, then adjust original target pos
				//by extending it further from this game obj targetPos + (targetPos - this game obj pos) until their distance on screen is larger than 100
				//hudInfo.originalSpawnPositionOffset.PositionOffset is the local position of the original target

				Vector2 thisGameObjScreenPos = mainCamera.WorldToScreenPoint(transform.position);
				Vector2 targetScreenPos = mainCamera.WorldToScreenPoint(transform.position + hudInfo.targetPos);
				Vector2 originalTargetScreenPos =
					mainCamera.WorldToScreenPoint(hudInfo.originalSpawnTransform.transform.position);


				if (Physics.Raycast(camTr.position, hudInfo.originalSpawnTransform.transform.position - camTr.position,
					    out hit, 100f, crossHairHUDManagedDetectLayerMask)) {
					if (!hit.collider.isTrigger && hit.collider.attachedRigidbody &&
					    hit.collider.attachedRigidbody.gameObject == gameObject) {
						needAdjust = true;

						Transform realHealthBarSpawnPoint = hudInfo.realSpawnPositionOffset.transform;
						if (Physics.Raycast(camTr.position, transform.position + hudInfo.targetPos - camTr.position,
							    out hit, 100f, crossHairHUDManagedDetectLayerMask)) {
							if (hit.collider.attachedRigidbody &&
							    hit.collider.attachedRigidbody.gameObject == gameObject) {
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


				float originalToScreenDistance =
					Vector2.Distance(thisGameObjScreenPos, originalTargetScreenPos);




				if (originalToScreenDistance < 40f) {
					needAdjust = true;
					float distance = Vector2.Distance(thisGameObjScreenPos, targetScreenPos);
					var position = transform.position + hudInfo.targetPos;
					float realWorldDistance = Vector3.Distance(position, transform.position);
					/*if (distance < 40f) {

						float requiredRealWorldDistance = (realWorldDistance * 40f) / distance;

				
						float distanceToMove = requiredRealWorldDistance - realWorldDistance;

						Vector3 direction = (position - transform.position).normalized;
				
						hudInfo.targetPos += direction * distanceToMove;
					}*/

					//always make the screen distance equal to 40
					float requiredRealWorldDistance = (realWorldDistance * 40f) / distance;
					float distanceToMove = requiredRealWorldDistance - realWorldDistance;
					Vector3 direction = (position - transform.position).normalized;
					hudInfo.targetPos += direction * distanceToMove;
				}
				
				if (!needAdjust) {
					hudInfo.targetPos = hudInfo.originalSpawnPositionOffset.PositionOffset;
				}

				hudInfo.realSpawnPositionOffset.PositionOffset = hudInfo.targetPos; 

			}



	
			//Vector3.Lerp(hudInfo.realSpawnPositionOffset.PositionOffset, hudInfo.targetPos, 3 * Time.fixedDeltaTime);
		}
		
		protected virtual void FixedUpdate() {
			//if (true) {
			
			//}
		}

		private void ReadyToRecycle() {
			readyToRecycled = true;
			gameObject.SetActive(false);
			StopAllCoroutines();
			BoundEntity.UnRegisterReadyToRecycle(OnEntityReadyToRecycle);
			OnReadyToRecycle();
		}

		protected virtual void OnEntityRecycled(IEntity ent) {
			if (autoDestroyWhenEntityRemoved) {
				RecycleToCache();
			}
		}
		
		protected new void RecycleToCache() {
			if (!readyToRecycled) {
				ReadyToRecycle();
			}
			base.RecycleToCache();
		}
		
		private void OnEntityReadyToRecycle(IEntity e) {
			if (autoDestroyWhenEntityRemoved) {
				ReadyToRecycle();
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

				//HUDManager.Singleton.DespawnHUDElement(tr, HUDCategory.InteractiveTag);
			//HUDManager.Singleton.DespawnHUDElement(nameTagFollowTransform, HUDCategory.NameTag);
			
			/*if (autoRemoveEntityWhenDestroyedOrRecycled) {
				entityModel.RemoveEntity(ID);
			}*/
			ID = null;
			BoundEntity = null;
			propertyBindings.Clear();
			crossHairHUDTimer = 0;
			interactiveHintToleranceTimer = 0;
			readyToRecycled = false;
			canInteractRc.Value = 2;
			currentInteractiveHint = null;
			interactiveHintHoldTimer = 0;
			unPointTriggered = false;
			unInteractTriggered = false;
			
		}

		protected virtual void OnReadyToRecycle() {
			//gameObject.SetActive(false);
			Transform tr = interactiveHintFollowTransform ? interactiveHintFollowTransform : transform;

			HUDManager.Singleton.DespawnHUDElement(tr, HUDCategory.InteractiveTag);
			DespawnHUDElement(nameTagFollowTransform, HUDCategory.NameTag);

			string[] keys = crossHairManagedHUDs.Keys.ToArray();
			foreach (string key in keys) {
				DespawnHUDElement(crossHairManagedHUDs[key].originalSpawnTransform,
					crossHairManagedHUDs[key].hudCategory);
			}
			crossHairManagedHUDs.Clear();
			isPointed = false;
			isInteractiveHintActive = false;
			unPointTriggered = false;
			unInteractTriggered = false;
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


		private BindableProperty<int> canInteractRc = new BindableProperty<int>(2);

		private void ChangeInteractRC(int value) {
			canInteractRc.Value = Mathf.Clamp(value, 0, 2);
		}
		public virtual void OnPlayerInteractiveZoneReachable(GameObject player, PlayerInteractiveZone zone) {
			if (hasInteractiveHint) {
				ChangeInteractRC(canInteractRc.Value - 1);
			}
		}
		
		
		

		public virtual void OnPlayerInteractiveZoneNotReachable(GameObject player, PlayerInteractiveZone zone) {
			if (hasInteractiveHint) {
				//canInteractRc.Value++;
				ChangeInteractRC(canInteractRc.Value + 1);
			}
		}
		
		protected InteractiveHint currentInteractiveHint;
		protected bool isInteractiveHintActive;
		private void OnCanInteractChanged(int oldVal, int newVal) {
			 if (!hasInteractiveHint) {
				return;
			}
			if (newVal <= 0 && oldVal > 0) {
				bool isActivePreviously = isInteractiveHintActive;
				isInteractiveHintActive = true;

				if (interactiveHintToleranceTimer <= 0f && !isActivePreviously) {
					Transform tr = interactiveHintFollowTransform ? interactiveHintFollowTransform : transform;
				
					GameObject hud = HUDManager.Singleton.SpawnHUDElement(tr, interactiveHintPrefabName,
						HUDCategory.InteractiveTag, true);
					
					
					if (hud) {
						currentInteractiveHint = hud.GetComponent<InteractiveHint>();
						if (currentInteractiveHint != null) {
							(InputAction action, string text, string control) = GetInteractHintInfo();
							currentInteractiveHint.SetHint(action, text, control);
						}
					}
					interactiveHintToleranceTimer = 0f;
				}

				
			}
			
			if (newVal > 0 && oldVal <= 0) {
				isInteractiveHintActive = false;
				unInteractTriggered = false;
			}
		}

		protected virtual (InputAction, string, string) GetInteractHintInfo() {
			string hint = interactiveHintHoldTime <= 0
				? Localization.Get("HINT_PRESS")
				: Localization.Get("HINT_HOLD");
			
			return (ClientInput.Singleton.FindActionInSharedActionMap("Interact"),
				Localization.Get(interactiveHintLocalizedKey), hint);
		}
		public virtual void OnPointByCrosshair() {
			bool isPointPreviously = isPointed;
			isPointed = true;
			
			if (showNameTagWhenPointed && !String.IsNullOrEmpty(nameTagPrefabName)) {
				if(nameTagFollowTransform && crossHairHUDTimer <= 0f && !isPointPreviously) {
					 (GameObject, CrossHairManagedHUDInfo) nameTagInfo =
						SpawnCrosshairResponseHUDElement(nameTagFollowTransform, nameTagPrefabName,
							HUDCategory.NameTag, nameTagAutoAdjustHeight);
					GameObject
						nameTag = nameTagInfo.Item1;
					
					if (nameTag) {
						INameTag nameTagComponent = nameTag.GetComponent<INameTag>();
						if (nameTagComponent != null) {
							nameTagComponent.SetName(BoundEntity.GetDisplayName());
						}
						nameTag.gameObject.SetActive(false);
						StartCoroutine(ShowNameTagDelayed(nameTag,nameTagInfo.Item2));
					}
				}
			}

			foreach (AbstractEntityViewController<T>.CrossHairManagedHUDInfo hudInfo in crossHairManagedHUDs.Values) {
				hudInfo.spawnedHUD.SetActive(true);
			}
			
			crossHairHUDTimer = 0f;
			if (hasInteractiveHint) {
				//canInteractRc.Value--;
				ChangeInteractRC(canInteractRc.Value - 1);
			}
		}
		
		public virtual void OnUnPointByCrosshair() {
			isPointed = false;
			unPointTriggered = false;

			if (hasInteractiveHint) {
				//canInteractRc.Value++;
				ChangeInteractRC(canInteractRc.Value + 1);
			}
		}
		
		

		public virtual void OnPlayerInInteractiveZone(GameObject player, PlayerInteractiveZone zone) {
			
		}

		public virtual void OnPlayerExitInteractiveZone(GameObject player, PlayerInteractiveZone zone) {
			
		}

		public IUnRegister RegisterOnEntityViewControllerInit(Action<IEntityViewController> callback) {
			onEntityVCInitCallback += callback;
			return new EntityVCOnRecycledUnRegister(this, callback);
		}

		public void UnRegisterOnEntityViewControllerInit(Action<IEntityViewController> callback) {
			onEntityVCInitCallback -= callback;
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