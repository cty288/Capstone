using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("explosionVFX", "explosionPos", "<ID>k__BackingField", "autoCreateNewEntityWhenStart", "showNameTagWhenPointed", "nameTagFollowTransform", "nameTagPrefabName", "nameTagAutoAdjustHeight", "hasInteractiveHint", "interactiveHintPrefabName", "interactiveHintLocalizedKey", "interactiveHintToleranceMaxTime", "interactiveHintFollowTransform", "interactiveHintHoldTime", "autoDestroyWhenEntityRemoved", "crossHairHUDToleranceMaxTime", "crossHairHUDToleranceScreenDistanceFactor", "onRecycledEvent", "onAllocateEvent", "Pool")]
	public class ES3UserType_MineralRobotEntityViewController : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_MineralRobotEntityViewController() : base(typeof(_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)obj;
			
			writer.WritePrivateFieldByRef("explosionVFX", instance);
			writer.WritePrivateFieldByRef("explosionPos", instance);
			writer.WritePrivateField("<ID>k__BackingField", instance);
			writer.WritePrivateField("autoCreateNewEntityWhenStart", instance);
			writer.WritePrivateField("showNameTagWhenPointed", instance);
			writer.WritePrivateFieldByRef("nameTagFollowTransform", instance);
			writer.WritePrivateField("nameTagPrefabName", instance);
			writer.WritePrivateField("nameTagAutoAdjustHeight", instance);
			writer.WritePrivateField("hasInteractiveHint", instance);
			writer.WritePrivateField("interactiveHintPrefabName", instance);
			writer.WritePrivateField("interactiveHintLocalizedKey", instance);
			writer.WritePrivateField("interactiveHintToleranceMaxTime", instance);
			writer.WritePrivateFieldByRef("interactiveHintFollowTransform", instance);
			writer.WritePrivateField("interactiveHintHoldTime", instance);
			writer.WritePrivateField("autoDestroyWhenEntityRemoved", instance);
			writer.WritePrivateField("crossHairHUDToleranceMaxTime", instance);
			writer.WritePrivateField("crossHairHUDToleranceScreenDistanceFactor", instance);
			writer.WritePrivateField("onRecycledEvent", instance);
			writer.WritePrivateField("onAllocateEvent", instance);
			writer.WritePropertyByRef("Pool", instance.Pool);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "explosionVFX":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("explosionVFX", reader.Read<UnityEngine.GameObject>(), instance);
					break;
					case "explosionPos":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("explosionPos", reader.Read<UnityEngine.Transform>(), instance);
					break;
					case "<ID>k__BackingField":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("<ID>k__BackingField", reader.Read<System.String>(), instance);
					break;
					case "autoCreateNewEntityWhenStart":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("autoCreateNewEntityWhenStart", reader.Read<System.Boolean>(), instance);
					break;
					case "showNameTagWhenPointed":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("showNameTagWhenPointed", reader.Read<System.Boolean>(), instance);
					break;
					case "nameTagFollowTransform":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("nameTagFollowTransform", reader.Read<UnityEngine.Transform>(), instance);
					break;
					case "nameTagPrefabName":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("nameTagPrefabName", reader.Read<System.String>(), instance);
					break;
					case "nameTagAutoAdjustHeight":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("nameTagAutoAdjustHeight", reader.Read<System.Boolean>(), instance);
					break;
					case "hasInteractiveHint":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("hasInteractiveHint", reader.Read<System.Boolean>(), instance);
					break;
					case "interactiveHintPrefabName":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("interactiveHintPrefabName", reader.Read<System.String>(), instance);
					break;
					case "interactiveHintLocalizedKey":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("interactiveHintLocalizedKey", reader.Read<System.String>(), instance);
					break;
					case "interactiveHintToleranceMaxTime":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("interactiveHintToleranceMaxTime", reader.Read<System.Single>(), instance);
					break;
					case "interactiveHintFollowTransform":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("interactiveHintFollowTransform", reader.Read<UnityEngine.Transform>(), instance);
					break;
					case "interactiveHintHoldTime":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("interactiveHintHoldTime", reader.Read<System.Single>(), instance);
					break;
					case "autoDestroyWhenEntityRemoved":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("autoDestroyWhenEntityRemoved", reader.Read<System.Boolean>(), instance);
					break;
					case "crossHairHUDToleranceMaxTime":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("crossHairHUDToleranceMaxTime", reader.Read<System.Single>(), instance);
					break;
					case "crossHairHUDToleranceScreenDistanceFactor":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("crossHairHUDToleranceScreenDistanceFactor", reader.Read<System.Single>(), instance);
					break;
					case "onRecycledEvent":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("onRecycledEvent", reader.Read<UnityEngine.Events.UnityEvent>(), instance);
					break;
					case "onAllocateEvent":
					instance = (_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController)reader.SetPrivateField("onAllocateEvent", reader.Read<UnityEngine.Events.UnityEvent>(), instance);
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


	public class ES3UserType_MineralRobotEntityViewControllerArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_MineralRobotEntityViewControllerArray() : base(typeof(_02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill.MineralRobotEntityViewController[]), ES3UserType_MineralRobotEntityViewController.Instance)
		{
			Instance = this;
		}
	}
}