using _02._Scripts.Runtime.Baits.Commands;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.Utilities.AnimatorSystem;
using UnityEngine;

namespace Runtime.GameResources.ViewControllers {
	public interface IInHandDeployableResourceViewController : IInHandResourceViewController {
		public void OnDeployFailureReasonChanged(DeployFailureReason lastReason, DeployFailureReason currentReason);
		void OnDeploy();
		bool RemoveAfterDeploy { get; set; }
	}

	public abstract class AbstractInHandDeployableResourceViewController<T> :
		AbstractPickableInHandResourceViewController<T>,
		IInHandDeployableResourceViewController where T : class, IResourceEntity, new() {

		public override void OnItemStartUse() {
			
		}

		public override void OnItemStopUse() {
			
		}

		public override void OnItemUse() {
			
		}

	

		public override void OnStopHold() {
			this.SendCommand(SetDeployStatusHintCommand.Allocate(null));
			base.OnStopHold();
		}


		public virtual void OnDeployFailureReasonChanged(DeployFailureReason lastReason, DeployFailureReason currentReason) {
			if (currentReason == DeployFailureReason.NA || currentReason == DeployFailureReason.NoFailure) {
				this.SendCommand(SetDeployStatusHintCommand.Allocate(null));
			}else if (currentReason == DeployFailureReason.SlopeTooSteep ||
			          currentReason == DeployFailureReason.Obstructed || currentReason == DeployFailureReason.InAir) {
				this.SendCommand(SetDeployStatusHintCommand.Allocate("DEPLOY_ERROR_LOCATION"));
			}
		}

		public abstract void OnDeploy();
		public abstract bool RemoveAfterDeploy { get; set; }
	}
}