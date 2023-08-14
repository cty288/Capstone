using System;
using Runtime.DataFramework.Properties;

namespace Runtime.DataFramework.ViewControllers.Enemies {
	[AttributeUsage(AttributeTargets.Property)]
	public class BindAttribute : Attribute
	{
		public PropertyNameInfo PropertyName { get; private set; }
		public string GetterMethodName { get; set; } 

		public string OnChanged{ get; set; }
		public BindAttribute(PropertyName propertyName)
		{
			PropertyName = new PropertyNameInfo(propertyName);
		}
		
		public BindAttribute(PropertyName propertyName, string getterMethodName) {
			PropertyName = new PropertyNameInfo(propertyName);
			GetterMethodName = getterMethodName;
		}
		
		public BindAttribute(PropertyName propertyName, string getterMethodName, string onChangedMethodName)
		{
			PropertyName = new PropertyNameInfo(propertyName);
			GetterMethodName = getterMethodName;
			OnChanged = onChangedMethodName;
		}
		
		public BindAttribute(string propertyFullNaame) {
			PropertyName = new PropertyNameInfo(propertyFullNaame);
		}
		
		public BindAttribute(string propertyFullNaame, string getterMethodName) {
			PropertyName = new PropertyNameInfo(propertyFullNaame);
			GetterMethodName = getterMethodName;
		}
		
		public BindAttribute(string propertyFullNaame, string getterMethodName, string onChangedMethodName)
		{
			PropertyName = new PropertyNameInfo(propertyFullNaame);
			GetterMethodName = getterMethodName;
			OnChanged = onChangedMethodName;
		}
		
	}
	
	
	[AttributeUsage(AttributeTargets.Property)]
	public class BindCustomDataAttribute : BindAttribute {
		public string CustomDataName { get; private set; }
		public BindCustomDataAttribute(string customPropertyName, string customPropertyDataName) : 
		base($"custom_properties.{customPropertyName}.{customPropertyDataName}"){
			
		}
		
		public BindCustomDataAttribute(string customPropertyName, string customPropertyDataName, string getterMethodName) : 
			base($"custom_properties.{customPropertyName}.{customPropertyDataName}", getterMethodName){
			
		}
		
		public BindCustomDataAttribute(string customPropertyName, string customPropertyDataName, string getterMethodName, string onChangedMethodName) : 
			base($"custom_properties.{customPropertyName}.{customPropertyDataName}", getterMethodName, onChangedMethodName){
			
		}
	}
}