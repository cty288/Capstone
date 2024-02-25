using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Skills.Model.Builders;
using _02._Scripts.Runtime.Skills.Model.Instance;
using _02._Scripts.Runtime.Skills.ViewControllers.Base;
using DG.Tweening;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.GameResources.Model.Base;
using Runtime.Player.ViewControllers;
using Runtime.Utilities;
using Runtime.Utilities.AnimatorSystem;
using Runtime.Weapons.ViewControllers;
using UnityEngine;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances {
	public class GrendateSkillInHandViewController : ThownSkillInHandViewController<GrenadeSkill> {
		protected override float range => BoundEntity.GetCustomPropertyOfCurrentLevel<float>("explosion_radius");
		protected override void OnInitThrownGrenade(GameObject obj, params Collider[] ignoredColliders) {
			ThrownGrenadeViewController grenadeViewController = obj.GetComponent<ThrownGrenadeViewController>();
			grenadeViewController.Init(BoundEntity.CurrentFaction.Value,
				BoundEntity.GetCustomPropertyOfCurrentLevel<int>("explosion_damage"),
				range,
				gameObject,
				BoundEntity,
				ignoredColliders);
		}
	}
}