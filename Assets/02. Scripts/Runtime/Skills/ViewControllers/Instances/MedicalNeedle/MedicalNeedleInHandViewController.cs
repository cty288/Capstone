using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.Skills.Model.Builders;
using _02._Scripts.Runtime.Skills.Model.Instance;
using _02._Scripts.Runtime.Skills.ViewControllers.Base;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;
using Runtime.Player;
using Runtime.Player.ViewControllers;
using Runtime.Utilities;
using Runtime.Utilities.AnimatorSystem;
using UnityEngine;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.MedicalNeedle {
	public class MedicalNeedleInHandViewController  : AbstractInHandSkillViewController<MedicalNeedleSkill>  {
		private IGamePlayerModel playerModel;
		private bool usedBefore = false;
		private IBuffSystem buffSystem;
		protected override void Awake() {
			base.Awake();
			playerModel = this.GetModel<IGamePlayerModel>();
			buffSystem = this.GetSystem<IBuffSystem>();
		}

		protected override void OnBindEntityProperty() {
			
		}

		protected override void OnEntityStart() {
			base.OnEntityStart();
			this.RegisterEvent<OnPlayerAnimationEvent>(OnPlayerAnimationEvent)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		}

		private void OnPlayerAnimationEvent(OnPlayerAnimationEvent e) {
			if (e.AnimationName == "OnNeedleUse") {
				UseSkill(OnUseSuccess);
			}
		}

		private void OnUseSuccess() {
			int amount = BoundEntity.GetCustomPropertyOfCurrentLevel<int>("healing_amount");
			IPlayerEntity player = playerModel.GetPlayer();
			player.Heal(amount, player);

			if (BoundEntity.GetLevel() >= 3) {
				float buffDuration = BoundEntity.GetCustomPropertyOfCurrentLevel<float>("buff_duration");
				int buffAmount = BoundEntity.GetCustomPropertyOfCurrentLevel<int>("buff_effect");
				RecoveryBuff buff = RecoveryBuff.Allocate(player, player, buffDuration, buffAmount);
				buffSystem.AddBuff(player, buff);
			}
		}

		// public override void OnItemAltUse() { }
		
		public override void OnItemStartUse() {
			//this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Shoot", AnimationEventType.Trigger, 0));
		}

		public override void OnItemStopUse() {
			if (usedBefore) {
				return;
			}

			Debug.Log("Medical Needle ItemStopUse");
			usedBefore = true;
			this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("ItemUse", AnimationEventType.Trigger, 0));
		}

		public override void OnItemUse() {
		
		}

		protected override IResourceEntity OnInitSkillEntity(SkillBuilder<MedicalNeedleSkill> builder) {
			return builder.FromConfig().Build();
		}

		public override void OnRecycled() {
			base.OnRecycled();
			usedBefore = false;
			this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Shoot", AnimationEventType.ResetTrigger, 0));
		}
	}
}