using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.Skills.Model.Builders;
using _02._Scripts.Runtime.Skills.Model.Instance;
using _02._Scripts.Runtime.Skills.Model.Instances.AdrenalineSkill;
using _02._Scripts.Runtime.Skills.ViewControllers.Base;
using _02._Scripts.Runtime.Skills.ViewControllers.Instances.MedicalNeedle;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;
using Runtime.Player;
using Runtime.Player.ViewControllers;
using Runtime.Utilities;
using Runtime.Utilities.AnimatorSystem;
using UnityEngine;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.AdrenalineSkill {
	public class AdrenalineSkillInHandViewController : AbstractInHandSkillViewController<Model.Instance.AdrenalineSkill.AdrenalineSkill> {
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
			int buffLevel = BoundEntity.GetCustomPropertyOfCurrentLevel<int>("buff_level");
			IPlayerEntity player = playerModel.GetPlayer();
			StimulatedBuff buff = StimulatedBuff.Allocate(player,player, buffLevel);
			if(!buffSystem.AddBuff(player, player, buff)) {
				buff.RecycleToCache();
			}
		}

		// public override void OnItemAltUse() { }
		
		public override void OnItemStartUse() {
			
		}

		public override void OnItemStopUse() {
			if (usedBefore) {
				return;
			}
			
			usedBefore = true;
			this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("ItemUse", AnimationEventType.Trigger, 0));
		}

		public override void OnItemUse() {
		
		}

		protected override IResourceEntity OnInitSkillEntity(SkillBuilder<Model.Instance.AdrenalineSkill.AdrenalineSkill> builder) {
			return builder.FromConfig().Build();
		}

		public override void OnRecycled() {
			base.OnRecycled();
			usedBefore = false;
			this.SendCommand<PlayerAnimationCommand>
				(PlayerAnimationCommand.Allocate("Shoot", AnimationEventType.ResetTrigger, 0));
		}
	}
}