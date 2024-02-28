using _02._Scripts.Runtime.Baits.Commands;
using _02._Scripts.Runtime.Skills.Model.Builders;
using _02._Scripts.Runtime.Skills.Model.Instance.TurretSkill;
using _02._Scripts.Runtime.Skills.ViewControllers.Base;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Player.ViewControllers;
using Runtime.Utilities;
using Runtime.Utilities.AnimatorSystem;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.TurretSkill {
	public class TurretSkillInHandViewController : AbstractInHandSkillViewController<TurretSkillEntity>, IInHandDeployableResourceViewController {
		private bool usedBefore = false;
		private IDeployableResourceViewController deployableResourceViewController;
		protected override void OnBindEntityProperty() {
			
		}

		protected override void OnEntityStart() {
			base.OnEntityStart();
			this.RegisterEvent<OnPlayerAnimationEvent>(OnPlayerAnimationEvent)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			
			
		}

		

		private void OnPlayerAnimationEvent(OnPlayerAnimationEvent e) {
			if (e.AnimationName == "OnRemoteControllerUse") {
				UseSkill(OnUseSuccess);
			}
		}

		public override void OnItemStartUse() {
			
		}

		public override void OnItemStopUse() {
			//UseSkill(OnUseSuccess);
			
		}

		private void OnUseSuccess() {
			deployableResourceViewController?.OnDeploy();
			deployableResourceViewController = null;
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

		public void OnDeploy(IDeployableResourceViewController deployableResource) {
			if (usedBefore) {
				return;
			}
			usedBefore = true;
			this.SendCommand<PlayerAnimationCommand>
				(PlayerAnimationCommand.Allocate("ItemUse", AnimationEventType.Trigger, 0));
			
			
			deployableResourceViewController = deployableResource;
		}

		public override void OnRecycled() {
			base.OnRecycled();
			usedBefore = false;
			deployableResourceViewController = null;
		}


		public bool RemoveAfterDeploy { get; set; } = false;
	}
}