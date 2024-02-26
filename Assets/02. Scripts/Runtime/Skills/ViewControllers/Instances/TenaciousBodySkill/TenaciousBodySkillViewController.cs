using _02._Scripts.Runtime.Skills.Model.Builders;
using _02._Scripts.Runtime.Skills.ViewControllers.Base;
using Runtime.GameResources.Model.Base;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.TenaciousBodySkill {
	public class TenaciousBodySkillViewController : AbstractInHandSkillViewController<Model.Instance.TenaciousBodySkill.TenaciousBodySkill> {
		protected override void OnBindEntityProperty() {
			
		}

		public override void OnItemStartUse() {
			
		}

		public override void OnItemStopUse() {
			
		}

		public override void OnItemUse() {
			
		}

		protected override IResourceEntity OnInitSkillEntity(SkillBuilder<Model.Instance.TenaciousBodySkill.TenaciousBodySkill> builder) {
			return builder.FromConfig().Build();
		}
	}
}