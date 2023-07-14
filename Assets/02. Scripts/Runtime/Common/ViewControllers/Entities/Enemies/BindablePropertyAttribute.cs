using System;

namespace _02._Scripts.Runtime.Common.ViewControllers.Entities.Enemies {
	[AttributeUsage(AttributeTargets.Property)]
	public class BindablePropertyAttribute : Attribute
	{
		public PropertyName PropertyName { get; private set; }
		public string GetterMethodName { get; set; } 

		public BindablePropertyAttribute(PropertyName propertyName)
		{
			PropertyName = propertyName;
		}
		
		public BindablePropertyAttribute(PropertyName propertyName, string getterMethodName)
		{
			PropertyName = propertyName;
			GetterMethodName = getterMethodName;
		}
	}
}