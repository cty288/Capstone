using _02._Scripts.Runtime.Skills.Model.Builders;
using _02._Scripts.Runtime.Skills.Model.Instance;
using _02._Scripts.Runtime.Skills.ViewControllers.Base;
using Runtime.GameResources.Model.Base;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances {
	public class GrendateSkillInHandViewController : AbstractInHandSkillViewController<GrenadeSkill> {
		protected override void OnBindEntityProperty() {
			
		}

		public override void OnItemStartUse() {
			
		}

		

		public override void OnItemStopUse() {
			throw new System.NotImplementedException();
		}

		public override void OnItemUse() {
			throw new System.NotImplementedException();
		}

		protected override IResourceEntity OnInitSkillEntity(SkillBuilder<GrenadeSkill> builder) {
			throw new System.NotImplementedException();
		}
	}
}