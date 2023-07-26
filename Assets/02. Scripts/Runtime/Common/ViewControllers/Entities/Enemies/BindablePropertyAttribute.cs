using System;

namespace _02._Scripts.Runtime.Common.ViewControllers.Entities.Enemies {
	[AttributeUsage(AttributeTargets.Property)]
	public class BindablePropertyAttribute : Attribute
	{
		public PropertyNameInfo PropertyName { get; private set; }

		public PropertyNameInfo PropertyNamew;
		public string GetterMethodName { get; set; } 

		public string OnChanged{ get; set; }
		public BindablePropertyAttribute(PropertyName propertyName)
		{
			PropertyName = new PropertyNameInfo(propertyName);
		}
		
		public BindablePropertyAttribute(PropertyName propertyName, string getterMethodName) {
			PropertyName = new PropertyNameInfo(propertyName);
			GetterMethodName = getterMethodName;
		}
		
		public BindablePropertyAttribute(PropertyName propertyName, string getterMethodName, string onChangedMethodName)
		{
			PropertyName = new PropertyNameInfo(propertyName);
			GetterMethodName = getterMethodName;
			OnChanged = onChangedMethodName;
		}
		
		public BindablePropertyAttribute(string propertyFullNaame) {
			PropertyName = new PropertyNameInfo(propertyFullNaame);
		}
		
		public BindablePropertyAttribute(string propertyFullNaame, string getterMethodName) {
			PropertyName = new PropertyNameInfo(propertyFullNaame);
			GetterMethodName = getterMethodName;
		}
		
		public BindablePropertyAttribute(string propertyFullNaame, string getterMethodName, string onChangedMethodName)
		{
			PropertyName = new PropertyNameInfo(propertyFullNaame);
			GetterMethodName = getterMethodName;
			OnChanged = onChangedMethodName;
		}
		
	}
}