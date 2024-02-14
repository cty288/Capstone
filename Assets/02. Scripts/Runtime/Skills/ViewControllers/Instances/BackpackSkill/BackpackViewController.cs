using _02._Scripts.Runtime.Skills.Model.Builders;
using _02._Scripts.Runtime.Skills.ViewControllers.Base;
using Runtime.GameResources.Model.Base;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.BackpackSkill {
	public class BackpackViewController : AbstractInHandSkillViewController<Model.Instance.BackpackSkill.BackpackSkill> {
		protected override void OnBindEntityProperty() {
			
		}

		public override void OnItemStartUse() {
			
		}

		public override void OnItemStopUse() {
			
		}

		public override void OnItemUse() {
			
		}

		protected override IResourceEntity OnInitSkillEntity(SkillBuilder<Model.Instance.BackpackSkill.BackpackSkill> builder) {
			return builder.FromConfig().Build();
		}
	}
}