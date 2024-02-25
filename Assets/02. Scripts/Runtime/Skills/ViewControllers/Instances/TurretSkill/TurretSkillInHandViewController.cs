using _02._Scripts.Runtime.Baits.Commands;
using _02._Scripts.Runtime.Skills.Model.Builders;
using _02._Scripts.Runtime.Skills.Model.Instance.TurretSkill;
using _02._Scripts.Runtime.Skills.ViewControllers.Base;
using MikroFramework.Architecture;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.TurretSkill {
	public class TurretSkillInHandViewController : AbstractInHandSkillViewController<TurretSkillEntity>, IInHandDeployableResourceViewController {
		protected override void OnBindEntityProperty() {
			
		}

		public override void OnItemStartUse() {
			
		}

		public override void OnItemStopUse() {
			//UseSkill(OnUseSuccess);
		}

		private void OnUseSuccess() {
			
		}

		public override void OnItemUse() {
			
		}

		protected override IResourceEntity OnInitSkillEntity(SkillBuilder<TurretSkillEntity> builder) {
			return builder.FromConfig().Build();
		}

		public override void OnStopHold() {
			this.SendCommand(SetDeployStatusHintCommand.Allocate(null));
			base.OnStopHold();
		}


		public void OnDeployFailureReasonChanged(DeployFailureReason lastReason, DeployFailureReason currentReason) {
			if (currentReason == DeployFailureReason.NA || currentReason == DeployFailureReason.NoFailure) {
				this.SendCommand(SetDeployStatusHintCommand.Allocate(null));
			}else if (currentReason == DeployFailureReason.SlopeTooSteep ||
			          currentReason == DeployFailureReason.Obstructed || currentReason == DeployFailureReason.InAir) {
				this.SendCommand(SetDeployStatusHintCommand.Allocate("DEPLOY_ERROR_LOCATION"));
			}else if (currentReason == DeployFailureReason.TurretReachMaxCount) {
				this.SendCommand(SetDeployStatusHintCommand.Allocate("DEPLOY_ERROR_TURRET_MAX_COUNT"));
			}
		}

		public void OnDeploy() {
			UseSkill(OnUseSuccess);
		}

		public bool RemoveAfterDeploy { get; set; } = false;
	}
}