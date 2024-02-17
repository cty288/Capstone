using UnityEngine;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.StunGrenadeSkill {
	public class StunGrenadeSkillInHandViewController : ThownSkillInHandViewController<Model.Instance.StunGrendateSkill.StunGrenadeSkill> {
		protected override float range => BoundEntity.GetCustomPropertyOfCurrentLevel<float>("range");
		protected override void OnInitThrownGrenade(GameObject obj, params Collider[] ignoredColliders) {
			ThrownStunGrenadeViewController grenadeViewController = obj.GetComponent<ThrownStunGrenadeViewController>();
			grenadeViewController.Init(BoundEntity.CurrentFaction.Value,
				0,
				range,
				gameObject,
				BoundEntity,
				ignoredColliders);

			grenadeViewController.SetBuffInto(BoundEntity.GetCustomPropertyOfCurrentLevel<float>("malfunction_time"),
				BoundEntity.GetCustomPropertyOfCurrentLevel<float>("powerless_time"),
				BoundEntity.GetCustomPropertyOfCurrentLevel<int>("powerless_level"));
		}
	}
}