using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("waypoints", "healthBarPrefabName", "healthBarSpawnPoint", "showDamageNumber", "<ID>k__BackingField", "autoCreateNewEntityWhenStart", "showNameTagWhenPointed", "nameTagFollowTransform", "nameTagPrefabName", "hasInteractiveHint", "interactiveHintPrefabName", "interactiveHintLocalizedKey", "autoDestroyWhenEntityRemoved", "crossHairHUDToleranceMaxTime", "crossHairHUDToleranceScreenDistanceFactor", "onRecycledEvent", "onAllocateEvent", "Pool")]
	public class ES3UserType_Bee : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Bee() : base(typeof(Runtime.Enemies.SmallEnemies.Bee)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (Runtime.Enemies.SmallEnemies.Bee)obj;
			
			writer.WritePrivateField("waypoints", instance);
			writer.WritePrivateField("healthBarPrefabName", instance);
			writer.WritePrivateFieldByRef("healthBarSpawnPoint", instance);
			writer.WritePrivateField("showDamageNumber", instance);
			writer.WritePrivateField("<ID>k__BackingField", instance);
			writer.WritePrivateField("autoCreateNewEntityWhenStart", instance);
			writer.WritePrivateField("showNameTagWhenPointed", instance);
			writer.WritePrivateFieldByRef("nameTagFollowTransform", instance);
			writer.WritePrivateField("nameTagPrefabName", instance);
			writer.WritePrivateField("hasInteractiveHint", instance);
			writer.WritePrivateField("interactiveHintPrefabName", instance);
			writer.WritePrivateField("interactiveHintLocalizedKey", instance);
			writer.WritePrivateField("autoDestroyWhenEntityRemoved", instance);
			writer.WritePrivateField("crossHairHUDToleranceMaxTime", instance);
			writer.WritePrivateField("crossHairHUDToleranceScreenDistanceFactor", instance);
			writer.WritePrivateField("onRecycledEvent", instance);
			writer.WritePrivateField("onAllocateEvent", instance);
			writer.WritePropertyByRef("Pool", instance.Pool);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (Runtime.Enemies.SmallEnemies.Bee)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "waypoints":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("waypoints", reader.Read<System.Collections.Generic.List<UnityEngine.GameObject>>(), instance);
					break;
					case "healthBarPrefabName":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("healthBarPrefabName", reader.Read<System.String>(), instance);
					break;
					case "healthBarSpawnPoint":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("healthBarSpawnPoint", reader.Read<UnityEngine.Transform>(), instance);
					break;
					case "showDamageNumber":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("showDamageNumber", reader.Read<System.Boolean>(), instance);
					break;
					case "<ID>k__BackingField":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("<ID>k__BackingField", reader.Read<System.String>(), instance);
					break;
					case "autoCreateNewEntityWhenStart":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("autoCreateNewEntityWhenStart", reader.Read<System.Boolean>(), instance);
					break;
					case "showNameTagWhenPointed":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("showNameTagWhenPointed", reader.Read<System.Boolean>(), instance);
					break;
					case "nameTagFollowTransform":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("nameTagFollowTransform", reader.Read<UnityEngine.Transform>(), instance);
					break;
					case "nameTagPrefabName":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("nameTagPrefabName", reader.Read<System.String>(), instance);
					break;
					case "hasInteractiveHint":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("hasInteractiveHint", reader.Read<System.Boolean>(), instance);
					break;
					case "interactiveHintPrefabName":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("interactiveHintPrefabName", reader.Read<System.String>(), instance);
					break;
					case "interactiveHintLocalizedKey":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("interactiveHintLocalizedKey", reader.Read<System.String>(), instance);
					break;
					case "autoDestroyWhenEntityRemoved":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("autoDestroyWhenEntityRemoved", reader.Read<System.Boolean>(), instance);
					break;
					case "crossHairHUDToleranceMaxTime":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("crossHairHUDToleranceMaxTime", reader.Read<System.Single>(), instance);
					break;
					case "crossHairHUDToleranceScreenDistanceFactor":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("crossHairHUDToleranceScreenDistanceFactor", reader.Read<System.Single>(), instance);
					break;
					case "onRecycledEvent":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("onRecycledEvent", reader.Read<UnityEngine.Events.UnityEvent>(), instance);
					break;
					case "onAllocateEvent":
					instance = (Runtime.Enemies.SmallEnemies.Bee)reader.SetPrivateField("onAllocateEvent", reader.Read<UnityEngine.Events.UnityEvent>(), instance);
					break;
					case "Pool":
						instance.Pool = reader.Read<MikroFramework.Pool.GameObjectPool>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_BeeArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_BeeArray() : base(typeof(Runtime.Enemies.SmallEnemies.Bee[]), ES3UserType_Bee.Instance)
		{
			Instance = this;
		}
	}
}