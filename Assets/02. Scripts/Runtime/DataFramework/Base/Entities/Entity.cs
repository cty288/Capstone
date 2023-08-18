using System;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Description;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.DataFramework.Entities {
	public interface IEntity: IPoolable, IHaveDescription, IHaveDisplayName  {
	
		public string EntityName { get; }
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
	

		/// <summary>
		/// Return a property if you know its type, return null if not found
		/// This will not retrieve those nested properties (in PropertyDict, PropertyList, etc.)
		/// To retrieve nested properties, use the one that takes full name instead
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public IPropertyBase GetProperty (Type type);
	
		public bool HasProperty(PropertyNameInfo nameInfo);
	
		public bool TryGetProperty (PropertyNameInfo nameInfo, out IPropertyBase property);

		public void OnLoadFromSave();
	
		public void Initialize();
	
		public string UUID { get; }

		public void OnAllocate();
	
	
		/// <summary>
		/// Set the base value of all properties with the given name
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="modifier"></param>
		/// <typeparam name="T"></typeparam>
	
		public void SetPropertyBaseValue<T>(PropertyNameInfo name, T value, IPropertyDependencyModifier<T> modifier = null);
	
		/// <summary>
		/// Set the modifier of all properties with the given name
		/// </summary>
		/// <param name="name"></param>
		/// <param name="modifier"></param>
		/// <typeparam name="T"></typeparam>
	
		public void SetPropertyModifier<T>(PropertyNameInfo name, IPropertyDependencyModifier<T> modifier);

		public void LoadPropertyBaseValueFromConfig();
	}




	public abstract class Entity :  IEntity  {
		public abstract string EntityName { get; protected set; }
	
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

	
		//protected abstract IPropertyBase[] OnGetOriginalProperties();
		[field: SerializeField]
		[field: ES3Serializable]
		public string UUID { get; protected set; }

		[field: ES3Serializable]
		private bool initialized = false;

		protected ConfigTable configTable;
		public Entity() {
			//configTable = ConfigDatas.Singleton.EnemyEntityConfigTable;
			configTable = GetConfigTable();
			OnRegisterProperties();
		}

		protected abstract ConfigTable GetConfigTable();

		public void OnLoadFromSave() {
			_allProperties.Clear();
		
		
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

		public void SetPropertyBaseValue<T>(PropertyNameInfo name, T value, IPropertyDependencyModifier<T> modifier = null) {
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

		public void LoadPropertyBaseValueFromConfig() {
			int i = 0;
			while (i < _allProperties.Count) {
				IPropertyBase property = _allProperties.ElementAt(i).Value;
				if(property is ILoadFromConfigProperty loadFromConfigProperty) {
					dynamic value = configTable?.Get(EntityName, loadFromConfigProperty.GetFullName().ToString());
					if (value != null) {
						loadFromConfigProperty.LoadFromConfig(value);
					}
				}
				i++;
			}

		}
	
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


		public IPropertyBase GetProperty(Type type) {
			if (cachedPropertyNames.TryGetValue(type, out string propertyName)) {
				return GetProperty(new PropertyNameInfo(propertyName));
			}
			return null;
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

			List<string> order = EntityPropertyDependencyCache.GetInitializationOrder(EntityName, _allProperties);
			foreach (string propertyName in order) {
				if (_allProperties.TryGetValue(propertyName, out IPropertyBase property)) {
					InitProperty(property);
				}
				else {
					Debug.LogError($"Property with name {propertyName} not found in entity {EntityName}");
				}
			}
			initialized = true;
		}

		protected void InitProperty(IPropertyBase property) {
			PropertyNameInfo[] dependencies = property.GetDependentProperties();
			if (dependencies != null) {
				IPropertyBase[] dependentProperties = new IPropertyBase[dependencies.Length];
				for (int i = 0; i < dependencies.Length; i++) {
					dependentProperties[i] = _allProperties[dependencies[i].GetFullName()];
				}
				property.Initialize(dependentProperties, EntityName);
			}
			else {
				property.Initialize(null, EntityName);
			}
		}



		public void OnRecycled() {
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
			OnRecycle();
		}

		[field: ES3Serializable]
		public bool IsRecycled { get; set; } = false;
		public void RecycleToCache() {
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
		
		
		public string GetDisplayName() {
			string displayName = Localization.Get($"{EntityName}_name");
			if (displayName != Localization.KeyNotFound) {
				return displayName;
			}

			return "";
		}
	}
}