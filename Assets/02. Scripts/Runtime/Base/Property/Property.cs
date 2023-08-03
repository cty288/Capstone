using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Base.Property;
using _02._Scripts.Runtime.Common.Properties;
using _02._Scripts.Runtime.Utilities.ConfigSheet;
using MikroFramework.BindableProperty;
using MikroFramework.Serializer;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;


public struct PropertyNameInfo {
	private string FullName;
	
	/// <summary>
	/// Use this constructor when the dependency is a sub property of some other properties (i.e. custom properties)
	/// </summary>
	/// <param name="fullName"></param>
	public PropertyNameInfo(string fullName) {
		FullName = fullName;
	}

	
	/// <summary>
	/// Use this constructor when the dependency is a property of the entity, not a sub property of some other properties
	/// </summary>
	/// <param name="propertyName"></param>
	public PropertyNameInfo(PropertyName propertyName) {
		FullName = propertyName.ToString();
	}
	
	/// <summary>
	/// Use this constructor to get the fullname of a property if you have the property
	/// </summary>
	/// <param name="property"></param>
	public PropertyNameInfo(IPropertyBase property) {
		FullName = property.GetFullName();
	}
	
	/// <summary>
	/// Use this constructor to get the fullname of a property if you have the property name and the parent name
	/// Do not include property name in the parent name
	/// </summary>
	/// <param name="property"></param>
	/// <param name="parentName"></param>
	public PropertyNameInfo(PropertyName propertyName, string parentName) {
		FullName = parentName + "." + propertyName.ToString();
	}
	
	public string GetFullName() {
		return FullName;
	}
}

public interface IPropertyBase {
	public PropertyName PropertyName { get; }
	
	public void OnSetFullName(string fullName);
	
	public string GetFullName();
	
	public object GetBaseValue();
	public void SetBaseValue(object value);
	public object GetInitialValue();
	public IBindableProperty GetRealValue();
	public PropertyNameInfo[] GetDependentProperties();
	
	void Initialize(IPropertyBase[] dependencies, string parentEntityName);
	
	public void OnRecycled();

	public IPropertyBase SetModifier<T>(IPropertyDependencyModifier<T> modifier);
	
	//public IPropertyBase[] GetSubProperties();
}



public interface ILoadFromConfigProperty: IPropertyBase {
	void LoadFromConfig(dynamic value);


}

public interface IHaveSubProperties : IPropertyBase{
	
	void OnSetChildFullName() {
		IPropertyBase[] childProperties = GetChildProperties();
		if(childProperties != null && childProperties.Length > 0) {
			for (int i = 0; i < childProperties.Length; i++) {
				childProperties[i].OnSetFullName(GetFullName() + "." + childProperties[i].PropertyName);
			}
		}
	}
	
	public IPropertyBase[] GetChildProperties();
	
	public void RegisterRequestRegisterProperty(Action<Type, IPropertyBase, string, bool, bool> requestRegisterProperty);

	public void UnregisterRequestRegisterProperty(Action<Type, IPropertyBase, string, bool, bool> requestRegisterProperty);

	public void OnLoadFromSavedData();
}

public interface IProperty<T> : IPropertyBase{
	
	
	/// <summary>
	/// Base Value is the initial configured base value of a specific property
	/// </summary>
	public T BaseValue { get; set; }
	
	/// <summary>
	/// Initial Value is the initial real value of this property when the bind entity is initialized. It
	/// is dependent on the base value and the modifiers. Not changed during the game
	/// </summary>
	public T InitialValue { get; set; }
	
	/// <summary>
	/// Real Value initialized to InitialValue. Is changed during the game
	/// </summary>
	public new BindableProperty<T> RealValue { get; }

	public void SetBaseValue(T value);
	
	//Dictionary<PropertyName, List<IPropertyModifier<T>>> DependentPropertiesAndModifiers { get; set; }

	object IPropertyBase.GetBaseValue() => BaseValue;
    object IPropertyBase.GetInitialValue() => InitialValue;

    IBindableProperty IPropertyBase.GetRealValue() => RealValue;  
    void IPropertyBase.SetBaseValue(object value) => SetBaseValue((T)value);

   
}

public abstract class Property<T> : IProperty<T> {
	[ES3Serializable]
	private bool initializedBefore = false;
	public PropertyName PropertyName => GetPropertyName();
	

	[field: ES3Serializable] 
	protected string fullName;


	[field: ES3Serializable]
	public virtual  T BaseValue { get; set; } = default;
	[field: ES3Serializable]
	public virtual  T InitialValue { get; set; } = default;
	
	[field: ES3Serializable]
	public virtual BindableProperty<T> RealValue { get; } = new BindableProperty<T>();
	
	

	public virtual void SetBaseValue(T value) {
		BaseValue = value;
	}
	
	public void OnSetFullName(string fullName) {
		this.fullName = fullName;
		if(this is IHaveSubProperties subProperties) {
			subProperties.OnSetChildFullName();
		}
	}

	

	public string GetFullName() {
		return fullName;
	}
	



	[field: ES3Serializable]
	protected IPropertyDependencyModifier<T> modifier;

	public virtual void OnRecycled() {
		initializedBefore = false;
		RealValue.UnRegisterAll();
		RealValue.Value = default;
		InitialValue = default;
		
	}

	public IPropertyBase SetModifier<ValueType>(IPropertyDependencyModifier<ValueType> modifier) {
		if (initializedBefore) {
			throw new Exception("Cannot set modifier after initialization");
		}
		if (typeof(ValueType) == typeof(T)) {
			this.modifier = (IPropertyDependencyModifier<T>) modifier;
		}
		else {
			throw new Exception("Value type mismatch for property " + PropertyName);
		}
		return this;
	}

	/// <summary>
	/// Using Get instead of property to avoid ES3 serialization
	/// </summary>
	/// <returns></returns>
	protected abstract IPropertyDependencyModifier<T> GetDefautModifier();
	protected abstract PropertyName GetPropertyName();
	
	/// <summary>
	/// Base constructor for ES3 serialization
	/// </summary>
	public Property() {
		modifier = GetDefautModifier();
	}
	
	

	public abstract PropertyNameInfo[] GetDependentProperties();

	public virtual void Initialize(IPropertyBase[] dependencies, string parentEntityName) {
		T targetValue;
		bool canClone = false;
		targetValue = OnClone(BaseValue);
		if (modifier != null) {
			targetValue = modifier.Modify(targetValue, dependencies, parentEntityName, fullName);
		}
		InitialValue = targetValue;
		RealValue.Value = OnClone(InitialValue);
		initializedBefore = true;
	}
	
	protected virtual T OnClone(T value) {
		if (typeof(T).IsClass && typeof(ICloneable).IsAssignableFrom(typeof(T))) {
			return (T) ((ICloneable) value).Clone();
		}
		
		return value;
	}
	
	//override operator 
	public static implicit operator T(Property<T> property) {
		return property.RealValue.Value;
	}

}


public abstract class AbstractLoadFromConfigProperty<T> : Property<T>, ILoadFromConfigProperty {
	
	
	public void LoadFromConfig(dynamic value) {
		if (value != null) {
			SetBaseValue(OnSetBaseValueFromConfig(value));
		}
	}
	

	public abstract T OnSetBaseValueFromConfig(dynamic value);
	
	public AbstractLoadFromConfigProperty() : base() {
		
	}
}


/// <summary>
/// Independent Properties are properties without any dependencies
/// </summary>
/// <typeparam name="T"></typeparam>

public abstract class IndependentProperty<T> : Property<T> {
	
	protected override IPropertyDependencyModifier<T> GetDefautModifier() {
		return null;
	}

	public override PropertyNameInfo[] GetDependentProperties() {
		return null;
	}

	protected IndependentProperty():base(){}
	
}

public abstract class IndependentLoadFromConfigProperty<T> : IndependentProperty<T>, ILoadFromConfigProperty {
	protected IndependentLoadFromConfigProperty():base(){}
	public void LoadFromConfig(dynamic value) {
		if (value != null) {
			SetBaseValue(OnSetBaseValueFromConfig(value));
		}
	}

	
	public abstract T OnSetBaseValueFromConfig(dynamic value);
}

