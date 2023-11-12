using _02._Scripts.Runtime.Skills.Model.Builders;
using _02._Scripts.Runtime.Skills.Model.Instance;
using _02._Scripts.Runtime.Skills.ViewControllers.Base;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;
using Runtime.Player;
using Runtime.Player.ViewControllers;
using Runtime.Utilities;
using Runtime.Utilities.AnimatorSystem;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.MedicalNeedle {
	public class MedicalNeedleInHandViewController  : AbstractInHandSkillViewController<MedicalNeedleSkill>  {
		private IGamePlayerModel playerModel;
		protected override void Awake() {
			base.Awake();
			playerModel = this.GetModel<IGamePlayerModel>();
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
		}

		public override void OnItemStartUse() {
			//this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Shoot", AnimationEventType.Trigger, 0));

		}

		public override void OnItemStopUse() {
			this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Shoot", AnimationEventType.Trigger, 0));
		}

		public override void OnItemUse() {
		
		}

		protected override IResourceEntity OnInitSkillEntity(SkillBuilder<MedicalNeedleSkill> builder) {
			return builder.FromConfig().Build();
		}

		public override void OnRecycled() {
			base.OnRecycled();
			
		}
	}
}