using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Pool;
using MikroFramework.Utilities;
using Polyglot;
using Runtime.DataFramework.Description;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.DataFramework.Entities {
	public interface IEntity: IPoolable, IHaveDescription, IHaveDisplayName, ICanGetUtility, ICanSendEvent  {
	
		public string EntityName { get;}
		
		public void OverrideEntityName(string name);
		
		/// <summary>
		/// Register a property to the entity. This must be called before the entity is initialized
		/// To register a property after the entity is initialized, use RegisterTempProperty
		/// </summary>
		/// <param name="property"></param>
		/// <param name="force"></param>
		/// <typeparam name="T"></typeparam>
		public void RegisterInitialProperty<T>(T property) where T : IPropertyBase;
	
		/// <summary>
		/// Register a property to the entity. The property will be removed once the entity is recycled
		/// </summary>
		/// <param name="property"></param>
		/// <param name="overriddenName"></param>
		/// <typeparam name="T"></typeparam>
		public void RegisterTempProperty<T>(T property, string overriddenName, bool isRoot, bool alsoInitialize = true) where T : IPropertyBase ;
	
		/// <summary>
		/// Get a property by name, return null if not found
		/// This will not retrieve those nested properties (in PropertyDict, PropertyList, etc.)
		/// To retrieve nested properties, use the one that takes full name instead
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IPropertyBase GetProperty(PropertyName name);
	
		/// <summary>
		/// Get a property by full name, return null if not found
		/// This can retrieve those nested properties (in PropertyDict, PropertyList, etc.)
		/// e.g. GetProperty("custom_properties.attack.damage")
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IPropertyBase GetProperty(PropertyNameInfo name);
	
		/// <summary>
		/// Return a property if you know its type, return null if not found
		/// This will not retrieve those nested properties (in PropertyDict, PropertyList, etc.)
		/// To retrieve nested properties, use the one that takes full name instead
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>

		public T GetProperty<T>() where T : class, IPropertyBase;
	
		/// <summary>
		/// Get a property by full name, return null if not found
		/// This can retrieve those nested properties (in PropertyDict, PropertyList, etc.)
		/// This will auto convert the property to the given type
		/// </summary>
		/// <param name="name"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetProperty<T>(PropertyNameInfo name) where T : class, IPropertyBase;
		
		public HashSet<IBuffedProperty<TDataType>> GetBuffedProperties<TDataType>(BuffTag buffTag);
		
		public HashSet<IBuffedProperty<TDataType>> GetBuffedProperties<TDataType>(params BuffTag[] buffTags);
		
		public bool OnValidateBuff(IBuff buff);
		
		public IPropertyBase[] GetProperties(Func<IPropertyBase, bool> predicate);
	
		public bool HasProperty(PropertyNameInfo nameInfo);
	
		public bool TryGetProperty (PropertyNameInfo nameInfo, out IPropertyBase property);

		public void OnLoadFromSave();
	
		public void Initialize();
	
		public string UUID { get; }

		public void OnAllocate();
		void OnAwake();
	
		/// <summary>
		/// Set the base value of all properties with the given name
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="modifier"></param>
		/// <typeparam name="T"></typeparam>
	
		public void SetPropertyBaseValue<T>(PropertyNameInfo name, T value,  PropertyModifier<T> modifier = null);

		[Obsolete(
			"Use SetPropertyBaseValue<T>(PropertyNameInfo name, T value,  PropertyModifier<T> modifier = null) instead")]
		public void SetPropertyBaseValue<T>(PropertyNameInfo name, T value, IPropertyDependencyModifier<T> modifier);

		[Obsolete("Use SetPropertyModifier<T>(PropertyNameInfo name, PropertyModifier<T> modifier) instead")]
		public void SetPropertyModifier<T>(PropertyNameInfo name, IPropertyDependencyModifier<T> modifier);
		/// <summary>
		/// Set the modifier of all properties with the given name
		/// </summary>
		/// <param name="name"></param>
		/// <param name="modifier"></param>
		/// <typeparam name="T"></typeparam>
		public void SetPropertyModifier<T>(PropertyNameInfo name, PropertyModifier<T> modifier);

		public void LoadPropertyBaseValueFromConfig(ConfigTable overrideTable = null);


		/// <summary>
		/// After the entity is built, or loaded from save, this will be called
		/// </summary>
		public void OnStart(bool isLoadedFromSave);

		public IUnRegister RegisterOnEntityRecycled(Action<IEntity> onEntityRecycled);

		public void UnRegisterOnEntityRecycled(Action<IEntity> onEntityRecycled);
		
		public void RegisterReadyToRecycle(Action<IEntity> onReadyToRecycle);
		
		public void UnRegisterReadyToRecycle(Action<IEntity> onReadyToRecycle);
		
		public void RegisterOnRecycleRCZeroRef(Action<IEntity> onZeroRef);
		
		public void UnRegisterOnRecycleRCZeroRef(Action<IEntity> onZeroRef);
		
		public void ReadyToRecycle(bool force = false);
		
		public void RetainRecycleRC();
		
		public void ReleaseRecycleRC();
	}
	
	
	public class EntityOnRecycledUnRegister : IUnRegister
	{
		private Action<IEntity> onEntityRecycled;

		private IEntity entity;
		
		public EntityOnRecycledUnRegister(IEntity entity, Action<IEntity> onEntityRecycled) {
			this.entity = entity;
			this.onEntityRecycled = onEntityRecycled;
		}

		public void UnRegister() {
			entity.UnRegisterOnEntityRecycled(onEntityRecycled);
			entity = null;
		}
	}

	public class EntityRecycleRC : SimpleRC {
		private Action<IEntity> onZeroRef;
		private IEntity entity;
		public EntityRecycleRC(IEntity entity) {
			this.entity = entity;
		}
		
		public void RegisterOnZeroRef(Action<IEntity> onZeroRef) {
			this.onZeroRef += onZeroRef;
		}
		
		public void UnRegisterOnZeroRef(Action<IEntity> onZeroRef) {
			this.onZeroRef -= onZeroRef;
		}
		
		public void UnRegisterAllOnZeroRef() {
			onZeroRef = null;
		}
		
		protected override void OnZeroRef() {
			base.OnZeroRef();
			onZeroRef?.Invoke(entity);
		}
	}

	public struct BuffTagInfo {
		public Type DataType;
		public HashSet<IBuffedProperty> BuffedProperties;
		
		public BuffTagInfo(Type dataType) {
			DataType = dataType;
			BuffedProperties = new HashSet<IBuffedProperty>();
		}
	}

	

	public abstract class Entity :  IEntity  {
		public abstract string EntityName { get; set; }

		[ES3Serializable] private string originalEntityName;
	
		[ES3NonSerializable]
		private Dictionary<string, IPropertyBase> _allProperties { get; } =
			new Dictionary<string, IPropertyBase>();

		/// <summary>
		/// Root properties are those that are registered to the entity directly. When the game starts,
		/// _properties will contain all root properties + all nested properties from saved data
		/// </summary>
		[field: ES3Serializable]
		private Dictionary<string, IPropertyBase> rootProperties { get; } =
			new Dictionary<string, IPropertyBase>();

		[field: ES3Serializable] 
		private HashSet<string> tempPropertyNames = new HashSet<string>();

		private static Dictionary<Type, string> cachedPropertyNames = new Dictionary<Type, string>();

		[ES3NonSerializable]
		private Dictionary<BuffTag, BuffTagInfo> buffTagToProperties =
			new Dictionary<BuffTag, BuffTagInfo>();
		
		[ES3NonSerializable]
		private Dictionary<IBuffedProperty, HashSet<BuffTag>> buffedPropertiesToTags =
			new Dictionary<IBuffedProperty, HashSet<BuffTag>>();

		[ES3NonSerializable]
		private Dictionary<int, HashSet<IBuffedProperty>> cachedBuffedPropertiesQuery =
			new Dictionary<int, HashSet<IBuffedProperty>>();

		//protected abstract IPropertyBase[] OnGetOriginalProperties();
		[field: SerializeField]
		[field: ES3Serializable]
		public string UUID { get; protected set; }

		[field: ES3Serializable]
		private bool initialized = false;

		protected ConfigTable configTable;

		protected Action<IEntity> onEntityRecycled;

		protected EntityRecycleRC recycleRC;

		protected Action<IEntity> onReadyToRecycleEvent = null;
		
		protected Action<IEntity> onRecycleRCZeroRef = null;
		
		private bool readyToRecycle = false;
		public Entity() {
			//configTable = ConfigDatas.Singleton.EnemyEntityConfigTable;
			originalEntityName = EntityName;
			recycleRC = new EntityRecycleRC(this);
			recycleRC.RegisterOnZeroRef((e) => {
				if (readyToRecycle) {
					onRecycleRCZeroRef?.Invoke(this);
				}
			});
			configTable = GetConfigTable();
			OnRegisterProperties();
			
		}

		protected abstract ConfigTable GetConfigTable();

		public void OnLoadFromSave() {
			_allProperties.Clear();
			buffTagToProperties.Clear();

			foreach (KeyValuePair<string,IPropertyBase> rootProperty in rootProperties) {
				IPropertyBase property = rootProperty.Value;
				_allProperties.Add(property.GetFullName(), property);
				if(property is IHaveSubProperties subProperties) {
					subProperties.OnLoadFromSavedData();
					IPropertyBase[] properties = subProperties.GetChildProperties();
				
					if (properties != null) {
						foreach (IPropertyBase subProperty in properties) {
							_allProperties.Add(subProperty.GetFullName(), subProperty);
						}
					}
				}
			}
		
		
			foreach (IPropertyBase subProperty in _allProperties.Values) {
				if (subProperty is IHaveSubProperties s) {
					s.RegisterRequestRegisterProperty(DoRegisterNonRootProperty);
				}
			}
			OnAwake();
			OnStart(true);
		}


		public void OverrideEntityName(string name) {
			if (initialized) {
				Debug.LogError("Cannot override entity name after entity is initialized");
				return;
			}
			EntityName = name;
		}

		public void RegisterInitialProperty<T>(T property) where T : IPropertyBase {
			if (property is ICustomProperty) {
				Debug.LogError(
					$"Property {property.PropertyName.ToString()} is a custom property! Please add to your custom properties list! instead of registering it to the entity directly!");
				return;
			}

			if (initialized){
				Debug.LogError("Cannot register property after entity is initialized");
				return;
			}

			string name = property.PropertyName.ToString();
			DoRegisterProperty( typeof(T), property , name, false, false, true);
		}
	
		public void RegisterTempProperty<T>(T property, string overriddenName, bool isRoot, bool alsoInitialize = true) where T : IPropertyBase{
			string name = String.IsNullOrEmpty(overriddenName) ? property.PropertyName.ToString() : overriddenName;
			DoRegisterProperty(typeof(T), property, name, true, alsoInitialize, isRoot);
		}

		private void DoRegisterNonRootProperty(Type registeredType, IPropertyBase property, string name, bool isTemp,
			bool alsoInitialize) {
			DoRegisterProperty(registeredType, property, name, isTemp, alsoInitialize, false);
		}
	
		private void DoRegisterProperty(Type registeredType, IPropertyBase property, string name, bool isTemp, bool alsoInitialize, bool isRoot) {
		
		
			property.OnSetFullName(name);
		
		
			List<IPropertyBase> allProperties = new List<IPropertyBase>() {property};
			if(property is IHaveSubProperties subProperties) {
				IPropertyBase[] properties = subProperties.GetChildProperties();
				if (properties != null) {
					allProperties.AddRange(properties);
				}
			}

			int i = 0;
			foreach (IPropertyBase subProperty in allProperties) {
				string subPropertyName = subProperty.GetFullName();
				if (this._allProperties.ContainsKey(subPropertyName)) {
					throw new Exception($"Property {subPropertyName} already exists in entity {EntityName}!");
				}else {
					this._allProperties[subPropertyName] = subProperty;
					if (isRoot && i == 0) {
						rootProperties[subPropertyName] = subProperty;
					}
					if (isTemp) {
						tempPropertyNames.Add(subPropertyName);
					}
					if (subProperty is IHaveSubProperties s) {
						s.RegisterRequestRegisterProperty(DoRegisterNonRootProperty);
					}
				}

				i++;
			}
		

			if (!property.GetFullName().Contains('.')) {
				Type type = registeredType;
				cachedPropertyNames.TryAdd(type, property.GetFullName());
				Type concreteType = property.GetType();
				cachedPropertyNames.TryAdd(concreteType, property.GetFullName());
			}

			if (alsoInitialize) {
				InitProperty(property);
			}
		}

		protected abstract void OnRegisterProperties();
	

		public void OnAllocate() {
			this.UUID = System.Guid.NewGuid().ToString();
		}

		
		public void SetPropertyBaseValue<T>(PropertyNameInfo name, T value,  PropertyModifier<T> modifier = null) {
			if (initialized) {
				Debug.LogError("Cannot set property value after entity is initialized");
				return;
			}
			IPropertyBase property = GetProperty(name);
			if (property != null) {
				property.SetBaseValue(value);
				if (modifier != null) {
					property.SetModifier(modifier);
				}
			}else {
				Debug.LogError($"Property {name.ToString()} not found in entity {EntityName}");
			}
		}
		
		[Obsolete("Use SetPropertyBaseValue<T>(PropertyNameInfo name, T value,  PropertyModifier<T> modifier = null) instead")]
		public void SetPropertyBaseValue<T>(PropertyNameInfo name, T value,  IPropertyDependencyModifier<T> modifier) {
			if (initialized) {
				Debug.LogError("Cannot set property value after entity is initialized");
				return;
			}
			IPropertyBase property = GetProperty(name);
			if (property != null) {
				property.SetBaseValue(value);
				if (modifier != null) {
					property.SetModifier(modifier);
				}
			}else {
				Debug.LogError($"Property {name.ToString()} not found in entity {EntityName}");
			}
		}
		
		[Obsolete("Use SetPropertyModifier<T>(PropertyNameInfo name, PropertyModifier<T> modifier) instead")]
		public void SetPropertyModifier<T>(PropertyNameInfo name, IPropertyDependencyModifier<T> modifier) {
			if (initialized) {
				Debug.LogError("Cannot set property value after entity is initialized");
				return;
			}
			IPropertyBase property = GetProperty(name);
			if (property != null) {
				property.SetModifier(modifier);
			}else {
				Debug.LogError($"Property {name.ToString()} not found in entity {EntityName}");
			}
		}

		public void SetPropertyModifier<T>(PropertyNameInfo name, PropertyModifier<T> modifier) {
			if (initialized) {
				Debug.LogError("Cannot set property value after entity is initialized");
				return;
			}
			IPropertyBase property = GetProperty(name);
			if (property != null) {
				property.SetModifier(modifier);
			}else {
				Debug.LogError($"Property {name.ToString()} not found in entity {EntityName}");
			}
		}

		public void LoadPropertyBaseValueFromConfig(ConfigTable overrideTable = null) {
			int i = 0;
			ConfigTable targetTable = overrideTable ?? configTable;
			while (i < _allProperties.Count) {
				IPropertyBase property = _allProperties.ElementAt(i).Value;
				if(property is ILoadFromConfigProperty loadFromConfigProperty) {
					dynamic value = targetTable?.Get(EntityName, loadFromConfigProperty.GetFullName().ToString());
					if (value is not null) {
						loadFromConfigProperty.LoadFromConfig(value, this);
						
					}
				}
				i++;
			}

		}

		public virtual void OnStart(bool isLoadedFromSave) {
			OnEntityStart(isLoadedFromSave);
		}

		public IUnRegister RegisterOnEntityRecycled(Action<IEntity> onEntityRecycled) {
			this.onEntityRecycled += onEntityRecycled;
			return new EntityOnRecycledUnRegister(this, onEntityRecycled);
		}

		public void UnRegisterOnEntityRecycled(Action<IEntity> onEntityRecycled) {
			this.onEntityRecycled -= onEntityRecycled;
		}

		public void RegisterReadyToRecycle(Action<IEntity> onReadyToRecycle) {
			this.onReadyToRecycleEvent += onReadyToRecycle;
		}

		public void UnRegisterReadyToRecycle(Action<IEntity> onReadyToRecycle) {
			this.onReadyToRecycleEvent -= onReadyToRecycle;
		}

		public void RegisterOnRecycleRCZeroRef(Action<IEntity> onZeroRef) {
			this.onRecycleRCZeroRef += onZeroRef;
		}

		public void UnRegisterOnRecycleRCZeroRef(Action<IEntity> onZeroRef) {
			this.onRecycleRCZeroRef -= onZeroRef;
		}

		public void ReadyToRecycle(bool force = false) {
			readyToRecycle = true;
			this.onReadyToRecycleEvent?.Invoke(this);
			if (recycleRC.RefCount == 0 || force) {
				this.onRecycleRCZeroRef?.Invoke(this);
			}
		}

		public void RetainRecycleRC() {
			recycleRC.Retain();
		}
		
		
		public void ReleaseRecycleRC() {
			recycleRC.Release();
		}

		/// <summary>
		/// After the entity is built, or loaded from save, this will be called
		/// </summary>
		/// <param name="isLoadedFromSave"></param>
		protected abstract void OnEntityStart(bool isLoadedFromSave);

		public IPropertyBase GetProperty(PropertyName name) {
			if (_allProperties.TryGetValue(name.ToString(), out var property)) {
				return property;
			}

			return null;
		}

		public IPropertyBase GetProperty(PropertyNameInfo name) {
			if (_allProperties.TryGetValue(name.GetFullName(), out var property)) {
				return property;
			}
			return null;
		}

		public T GetProperty<T>() where T : class, IPropertyBase {
			if (cachedPropertyNames.TryGetValue(typeof(T), out string propertyName)) {
				return (T) GetProperty(new PropertyNameInfo(propertyName));
			}
			return null;
		}

		public T GetProperty<T>(PropertyNameInfo name) where T : class, IPropertyBase {
			if (_allProperties.TryGetValue(name.GetFullName(), out var property)) {
				return property as T;
			}
			return null;
		}

		public HashSet<IBuffedProperty<TDataType>> GetBuffedProperties<TDataType>(BuffTag buffTag) {
			HashSet<IBuffedProperty<TDataType>> res = new HashSet<IBuffedProperty<TDataType>>();
			
			if (buffTagToProperties.TryGetValue(buffTag, out BuffTagInfo buffTagInfo)) {
				foreach (IBuffedProperty buffedProperty in buffTagInfo.BuffedProperties) {
					res.Add((IBuffedProperty<TDataType>) buffedProperty);
				}
			}
			return res;
		}

		public HashSet<IBuffedProperty<TDataType>> GetBuffedProperties<TDataType>(params BuffTag[] buffTags) {
			
			HashSet<IBuffedProperty<TDataType>> res = new HashSet<IBuffedProperty<TDataType>>();

			//transform buffTags to bit mask
			int buffTagsBitMask = 0;
			if (buffTags != null) {
				foreach (BuffTag buffTag in buffTags) {
					buffTagsBitMask |= 1 << (int) buffTag;
				}
			}
			
			if (cachedBuffedPropertiesQuery.TryGetValue(buffTagsBitMask, out HashSet<IBuffedProperty> cachedProperties)) {
				foreach (IBuffedProperty property in cachedProperties) {
					res.Add((IBuffedProperty<TDataType>) property);
				}
				return res;
			}


			HashSet<IBuffedProperty> buffedProperties = new HashSet<IBuffedProperty>();
			foreach (IBuffedProperty propertyBase in buffedPropertiesToTags.Keys) {
				HashSet<BuffTag> tags = buffedPropertiesToTags[propertyBase];
				bool hasAllTags = true;

				if (buffTags != null) {
					foreach (BuffTag buffTag in buffTags) {
						if (!tags.Contains(buffTag)) {
							hasAllTags = false;
							break;
						}
					}
				}
				

				if (hasAllTags) {
					buffedProperties.Add(propertyBase);
					res.Add((IBuffedProperty<TDataType>) propertyBase);
				}
			}

			cachedBuffedPropertiesQuery[buffTagsBitMask] = buffedProperties;
			return res;
		}

		public virtual bool OnValidateBuff(IBuff buff) {
			return true;
		}


		public IPropertyBase GetProperty(Type type) {
			if (cachedPropertyNames.TryGetValue(type, out string propertyName)) {
				return GetProperty(new PropertyNameInfo(propertyName));
			}
			return null;
		}

		public IPropertyBase[] GetProperties(Func<IPropertyBase, bool> predicate) {
			return _allProperties.Values.Where(predicate).ToArray();
		}

		public bool HasProperty(PropertyNameInfo nameInfo) {
			return _allProperties.ContainsKey(nameInfo.GetFullName());
		}

		public bool TryGetProperty(PropertyNameInfo nameInfo, out IPropertyBase property) {
			if (_allProperties.TryGetValue(nameInfo.GetFullName(), out var p)) {
				property = p;
				return true;
			}
			property = null;
			return false;
		}




		public void Initialize() {
			if (initialized) {
				return;
			}
			
			OnInitModifiers();
			List<string> order = EntityPropertyDependencyCache.GetInitializationOrder(EntityName, _allProperties);
			foreach (string propertyName in order) {
				if (_allProperties.TryGetValue(propertyName, out IPropertyBase property)) {
					InitProperty(property);
				}
				else {
					Debug.LogWarning($"Property with name {propertyName} not found in entity {EntityName}");
				}
			}
			initialized = true;
		}

		public virtual void OnAwake() {
			buffTagToProperties.Clear();
			buffedPropertiesToTags.Clear();
			cachedBuffedPropertiesQuery.Clear();
			foreach (IPropertyBase subProperty in _allProperties.Values) {
				if (subProperty is IBuffedProperty buffedProperty) {
					HashSet<BuffTag> tags = buffedProperty.BuffTags;
					bool hasError = false;
					if (tags != null) {
						
						foreach (BuffTag tag in tags) {
							if (!buffTagToProperties.ContainsKey(tag)) {
								buffTagToProperties[tag] = new BuffTagInfo(buffedProperty.GetBaseValue().GetType());
							}
							
							Type buffedPropertyType = buffedProperty.GetBaseValue().GetType();
							if (buffTagToProperties[tag].DataType != buffedPropertyType) {
								Debug.LogError(
									$"Buffed property {buffedProperty.PropertyName} has different data type from other properties with the same buff tag {tag}, which is supposed to be {buffTagToProperties[tag].DataType}");
								hasError = true;
							}
							else {
								buffTagToProperties[tag].BuffedProperties.Add(buffedProperty);
							}
						}
					}
					
					if (hasError) {
						continue;
					}

					if (tags == null) {
						tags = new HashSet<BuffTag>();
					}
					buffedPropertiesToTags[buffedProperty] = tags;
				}
			}
		}

		protected void InitProperty(IPropertyBase property) {
			
			PropertyNameInfo[] dependencies = property.GetDependentProperties();
			if (dependencies != null) {
				IPropertyBase[] dependentProperties = new IPropertyBase[dependencies.Length];
				int index = 0;
				for (int i = 0; i < dependencies.Length; i++) {
					if (!_allProperties.ContainsKey(dependencies[i].GetFullName())) {
						continue;
					}
					dependentProperties[index] = _allProperties[dependencies[i].GetFullName()];
					index++;
				}
				property.Initialize(dependentProperties, EntityName);
			}
			else {
				property.Initialize(null, EntityName);
			}
		}

		protected abstract void OnInitModifiers();


		public void OnRecycled() {
			OnRecycle();
			
			onRecycleRCZeroRef = null;
			recycleRC.RefCount = 0;
			foreach (IPropertyBase property in _allProperties.Values) {
				property.OnRecycled();
			}
		
			foreach (string propertyName in tempPropertyNames) {
				IPropertyBase property = _allProperties[propertyName];
				if(property is IHaveSubProperties s) {
					s.UnregisterRequestRegisterProperty(DoRegisterNonRootProperty);
				}
				_allProperties.Remove(propertyName);
				rootProperties.Remove(propertyName);
			}
		
			tempPropertyNames.Clear();
			initialized = false;
			this.onEntityRecycled = null;
			EntityName = originalEntityName;
			this.onReadyToRecycleEvent = null;
			readyToRecycle = false;
			buffTagToProperties.Clear();
			cachedBuffedPropertiesQuery.Clear();
			buffedPropertiesToTags.Clear();
		}

		[field: ES3Serializable]
		public bool IsRecycled { get; set; } = false;
		public void RecycleToCache() {
			this.onEntityRecycled?.Invoke(this);
			OnDoRecycle();
		}

		public abstract void OnDoRecycle();

		public abstract void OnRecycle();
		
		
		public string GetDescription() {
			
			return OnGetDescription($"{EntityName}_desc");
		}

		/// <summary>
		/// Return the description of the entity
		/// </summary>
		/// <param name="defaultLocalizationKey">This is always equal to EntityName_desc. Use Localization.Get or Localization.GetFormat to retrieve its description based on the key <br />
		/// Or you can ignore defaultLocalizationKey can manually implement yours</param>
		protected abstract string OnGetDescription(string defaultLocalizationKey);
		
		
		public virtual string GetDisplayName() {
			string displayName = Localization.Get($"{EntityName}_name");
			if (displayName != Localization.KeyNotFound) {
				return displayName;
			}

			return "";
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}