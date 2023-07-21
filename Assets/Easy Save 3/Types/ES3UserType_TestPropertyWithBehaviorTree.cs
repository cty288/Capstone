using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("<ID>k__BackingField", "ID")]
	public class ES3UserType_TestPropertyWithBehaviorTree : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_TestPropertyWithBehaviorTree() : base(typeof(TestPropertyWithBehaviorTree)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (TestPropertyWithBehaviorTree)obj;
			
			writer.WritePrivateField("<ID>k__BackingField", instance);
			writer.WritePrivateProperty("ID", instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (TestPropertyWithBehaviorTree)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "<ID>k__BackingField":
					instance = (TestPropertyWithBehaviorTree)reader.SetPrivateField("<ID>k__BackingField", reader.Read<System.String>(), instance);
					break;
					case "ID":
					instance = (TestPropertyWithBehaviorTree)reader.SetPrivateProperty("ID", reader.Read<System.String>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_TestPropertyWithBehaviorTreeArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_TestPropertyWithBehaviorTreeArray() : base(typeof(TestPropertyWithBehaviorTree[]), ES3UserType_TestPropertyWithBehaviorTree.Instance)
		{
			Instance = this;
		}
	}
}