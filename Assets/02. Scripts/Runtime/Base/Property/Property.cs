using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Common.Properties;
using _02._Scripts.Runtime.Utilities.ConfigSheet;
using MikroFramework.BindableProperty;
using MikroFramework.Serializer;
using UnityEngine;

public interface IPropertyBase {
	public PropertyName PropertyName { get; }
	public object GetBaseValue();
	public void SetBaseValue(object value);
	public object GetInitialValue();
	public IBindableProperty GetRealValue();
	public PropertyName[] GetDependentProperties();
	
	void Initialize(IPropertyBase[] dependencies, string parentEntityName);
	
	public void OnRecycled();

	public IPropertyBase SetModifier<T>(IPropertyDependencyModifier<T> modifier);
	
	void LoadFromConfig(dynamic value);
}


public interface IProperty<T> : IPropertyBase{
	
	
	/// <summary>
	/// Base Value is the initial configured base value of a specific property
	/// </summary>
	public new T BaseValue { get; set; }
	
	/// <summary>
	/// Initial Value is the initial real value of this property when the bind entity is initialized. It
	/// is dependent on the base value and the modifiers. Not changed during the game
	/// </summary>
	public new T InitialValue { get; set; }
	
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
	public T BaseValue { get; set; } = default;
	[field: ES3Serializable]
	public T InitialValue { get; set; } = default;
	
	[field: ES3Serializable]
	public virtual BindableProperty<T> RealValue { get; } = new BindableProperty<T>();
	
	

	public virtual void SetBaseValue(T value) {
		BaseValue = value;
	}

	public void LoadFromConfig(dynamic value) {
		if (value != null) {
			SetBaseValue(OnSetBaseValueFromConfig(value));
		}
	}
	
	public abstract T OnSetBaseValueFromConfig(dynamic value);

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
	
	

	public abstract PropertyName[] GetDependentProperties();

	public virtual void Initialize(IPropertyBase[] dependencies, string parentEntityName) {
		T targetValue;
		bool canClone = false;
		targetValue = OnClone(BaseValue);
		if (modifier != null) {
			targetValue = modifier.Modify(targetValue, dependencies, parentEntityName, PropertyName);
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

/// <summary>
/// Independent Properties are properties without any dependencies
/// </summary>
/// <typeparam name="T"></typeparam>

public abstract class IndependentProperty<T> : Property<T> {
	
	protected override IPropertyDependencyModifier<T> GetDefautModifier() {
		return null;
	}

	public override PropertyName[] GetDependentProperties() {
		return null;
	}

	protected IndependentProperty():base(){}
	
}

