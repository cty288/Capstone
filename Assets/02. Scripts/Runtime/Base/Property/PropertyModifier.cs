using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPropertyDependencyModifier<T>{ 
    public T Modify(T propertyValue, IPropertyBase[] dep, string parentEntityName, PropertyName propertyName);
} 

/// <summary>
/// No need to be ES3 serialized
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class PropertyDependencyModifier<T> : IPropertyDependencyModifier<T>{
   
    private string parentEntityName;
    
    private PropertyName propertyName;

    private Dictionary<Type, IPropertyBase> dependencies = new Dictionary<Type, IPropertyBase>();
    
    public T Modify(T propertyValue, IPropertyBase[] dep, string parentEntityName, PropertyName propertyName) {
        this.parentEntityName = parentEntityName;
        this.propertyName = propertyName;
        foreach (IPropertyBase propertyBase in dep) {
            dependencies[propertyBase.GetType()] = propertyBase;
        }
        return OnModify(propertyValue);
    }
    
	
    /// <summary>
    /// Get parameters for this property modifier specific to this property + its entity + its dependent property from config file
    /// </summary>
    /// <param name="paramName"></param>
    /// <typeparam name="T">The type of the parameter to parse to</typeparam>
    /// <returns></returns>
    protected T GetParameter<T>(IPropertyBase dependency, string paramName) {
        string key = $"modifier_{parentEntityName}_{propertyName.ToString()}_{dependency.PropertyName.ToString()}_{paramName}";
        return default(T);
    }
    
    protected T GetDependency<T>() where T: IPropertyBase {
        if (dependencies.TryGetValue(typeof(T), out IPropertyBase propertyBase)) {
            return (T) propertyBase;
        }
        throw new Exception($"Property {propertyName.ToString()} of entity {parentEntityName} does not have dependency of type {typeof(T)}");
    }

    public abstract T OnModify(T propertyValue);

}

