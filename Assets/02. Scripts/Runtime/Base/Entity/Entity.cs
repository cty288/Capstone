using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Event;
using MikroFramework.IOC;
using MikroFramework.Pool;
using UnityEngine;

public interface IEntity: IPoolable {
	
	public string EntityName { get; }

	/// <summary>
	/// Get a property by name, return null if not found
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public IPropertyBase GetProperty (PropertyName name);
	
	/// <summary>
	/// Return a property if you know its type, return null if not found (recommended)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>

	public T GetProperty<T>() where T : class, IPropertyBase;
	
	public IPropertyBase GetProperty (Type type);

	public bool HasProperty (PropertyName name);
	
	public bool TryGetProperty (PropertyName name, out IPropertyBase property);

	public void Initialize();
	
	public string UUID { get; }

	public void OnAllocate();
	
	public void SetPropertyBaseValue<T>(PropertyName name, T value, IPropertyDependencyModifier<T> modifier = null);
	
	public void SetPropertyModifier<T>(PropertyName name, IPropertyDependencyModifier<T> modifier);

	public void LoadPropertyBaseValueFromConfig();
}




public abstract class Entity :  IEntity  {
	public abstract string EntityName { get; protected set; }

	[field: ES3Serializable]
	private Dictionary<PropertyName, IPropertyBase> _properties { get; } =
		new Dictionary<PropertyName, IPropertyBase>();
	
	private static Dictionary<Type, PropertyName> cachedPropertyNames = new Dictionary<Type, PropertyName>();

	//protected abstract IPropertyBase[] OnGetOriginalProperties();
	[field: SerializeField]
	[field: ES3Serializable]
	public string UUID { get; protected set; }

	[field: ES3Serializable]
	private bool initialized = false;

	public Entity() {
		OnRegisterProperties();
	}
	
	protected void RegisterProperty<T>(T property) where T : IPropertyBase {
		if (this._properties.ContainsKey(property.PropertyName)) {
			Debug.LogError($"Property {property.PropertyName.ToString()} already exists in entity {EntityName}");
			return;
		}
		this._properties.Add(property.PropertyName, property);
		Type type = typeof(T);
		cachedPropertyNames.TryAdd(type, property.PropertyName);
		Type concreteType = property.GetType();
		cachedPropertyNames.TryAdd(concreteType, property.PropertyName);
	}

	protected abstract void OnRegisterProperties();
	

	public void OnAllocate() {
		this.UUID = System.Guid.NewGuid().ToString();
	}

	public void SetPropertyBaseValue<T>(PropertyName name, T value, IPropertyDependencyModifier<T> modifier = null) {
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

	public void SetPropertyModifier<T>(PropertyName name, IPropertyDependencyModifier<T> modifier) {
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
		foreach (IPropertyBase property in _properties.Values) {
			property.OnLoadFromConfig(EntityName);
		}
		
	}


	/// <summary>
	/// Get a property by name, return null if not found
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public IPropertyBase GetProperty(PropertyName name) {
		if (_properties.ContainsKey(name)) {
			return _properties[name];
		}

		return null;
	}

	public T GetProperty<T>() where T : class, IPropertyBase {
		if (cachedPropertyNames.TryGetValue(typeof(T), out PropertyName propertyName)) {
			return (T) GetProperty(propertyName);
		}
		return null;
	}

	public IPropertyBase GetProperty(Type type) {
		if (cachedPropertyNames.TryGetValue(type, out PropertyName propertyName)) {
			return GetProperty(propertyName);
		}
		return null;
	}


	public bool HasProperty(PropertyName name) {
		return _properties.ContainsKey(name);
	}

	public bool TryGetProperty(PropertyName name, out IPropertyBase property) {
		return _properties.TryGetValue(name, out property);
	}

	public void Initialize() {
		if (initialized) {
			return;
		}
		List<PropertyName> order = EntityPropertyDependencyCache.GetInitializationOrder(EntityName, _properties);
		foreach (PropertyName propertyName in order) {
			if (_properties.TryGetValue(propertyName, out IPropertyBase property)) {
				PropertyName[] dependencies = property.GetDependentProperties();
				if (dependencies != null) {
					IPropertyBase[] dependentProperties = new IPropertyBase[dependencies.Length];
					for (int i = 0; i < dependencies.Length; i++) {
						dependentProperties[i] = _properties[dependencies[i]];
					}
					property.Initialize(dependentProperties, EntityName);
				}
				else {
					property.Initialize(null, EntityName);
				}
				
			}
			else {
				Debug.LogError($"Property with name {propertyName} not found in entity {EntityName}");
			}
		}
		initialized = true;
	}



	public void OnRecycled() {
		foreach (IPropertyBase property in _properties.Values) {
			property.OnRecycled();
		}
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
}