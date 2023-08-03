using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPropertyDependencyModifier<T>{ 
    public T Modify(T propertyValue, IPropertyBase[] dep, string parentEntityName, string propertyName);
} 

/// <summary>
/// No need to be ES3 serialized
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class PropertyDependencyModifier<T> : IPropertyDependencyModifier<T>{
   
    private string parentEntityName;
    
    private string propertyName;

    private Dictionary<string, IPropertyBase> dependenciesInFullname = new Dictionary<string, IPropertyBase>();

    protected Dictionary<Type, IPropertyBase> dependenciesInType = new Dictionary<Type, IPropertyBase>();

    public T Modify(T propertyValue, IPropertyBase[] dep, string parentEntityName, string propertyName) {
        this.parentEntityName = parentEntityName;
        this.propertyName = propertyName;
        foreach (IPropertyBase propertyBase in dep) {
            dependenciesInFullname[propertyBase.GetFullName()] = propertyBase;
            if(dependenciesInType.ContainsKey(propertyBase.GetType())) {
                Debug.LogWarning($"Property {propertyBase.GetFullName()} of entity {parentEntityName} has duplicate dependency of type {propertyBase.GetType()}. " +
                                 $"Please use the GetDependency overload that explicitly specify the dependency name to avoid ambiguity.");
            }
            else {
                dependenciesInType[propertyBase.GetType()] = propertyBase;
            }
        }
        T result = OnModify(propertyValue);
        dependenciesInType.Clear();
        dependenciesInFullname.Clear();
        return result;
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
    
    protected T GetDependency<T>(PropertyNameInfo nameInfo) where T: IPropertyBase {
        if (dependenciesInFullname.TryGetValue(nameInfo.GetFullName(), out IPropertyBase propertyBase)) {
            return (T) propertyBase;
        }
        throw new Exception($"Property {propertyName.ToString()} of entity {parentEntityName} does not have dependency of name {nameInfo.GetFullName()}");
    }
    
    protected IPropertyBase GetDependency(PropertyNameInfo nameInfo) {
        if (dependenciesInFullname.TryGetValue(nameInfo.GetFullName(), out IPropertyBase propertyBase)) {
            return propertyBase;
        }

        throw new Exception(
            $"Property {propertyName.ToString()} of entity {parentEntityName} does not have dependency of name {nameInfo.GetFullName()}");
    }
    
    
    /// <summary>
    /// Only use this when you are sure that there is only one dependency of this type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T GetDependency<T>() where T: IPropertyBase {
        if (dependenciesInType.TryGetValue(typeof(T), out IPropertyBase propertyBase)) {
            return (T) propertyBase;
        }
        throw new Exception($"Property {propertyName.ToString()} of entity {parentEntityName} does not have dependency of type {typeof(T)}");
    }
    
    

    public abstract T OnModify(T propertyValue);

}

