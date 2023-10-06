using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Player.Properties;
using Runtime.Temporary.Player;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.Player {
	public interface IPlayerEntity : ICreature, IEntity {
		IAccelerationForce GetAccelerationForce();
		IWalkSpeed GetWalkSpeed();
		ISprintSpeed GetSprintSpeed();
		ISlideSpeed GetSlideSpeed();
		IGroundDrag GetGroundDrag();
		IJumpForce GetJumpForce();
		IAdditionalGravity GetAdditionalGravity();
		IMaxSlideTime GetMaxSlideTime();
		ISlideForce GetSlideForce();
		IWallRunForce GetWallRunForce();

		MovementState GetMovementState();
		void SetMovementState(MovementState state);
		
		bool IsScopedIn();
		void SetScopedIn(bool state);
	}
	
	public class PlayerEntity : AbstractCreature, IPlayerEntity, ICanDealDamage {
		public override string EntityName { get; set; } = "Player";
		
		private IAccelerationForce accelerationForce;
		private IWalkSpeed walkSpeed;
		private ISprintSpeed sprintSpeed;
		private ISlideSpeed slideSpeed;
		private IGroundDrag groundDrag;
		private IJumpForce jumpForce;
		private IAdditionalGravity additionalGravity;
		private IMaxSlideTime maxSlideTime;
		private ISlideForce slideForce;
		private IWallRunForce wallRunForce;

		private MovementState movementState;
		private bool scopedIn;
		
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.PlayerEntityConfigTable;
		}

		public override void OnDoRecycle() {
			SafeObjectPool<PlayerEntity>.Singleton.Recycle(this as PlayerEntity);
		}

		public override void OnRecycle() {
			
		}
		protected override void OnInitModifiers(int rarity) {
            
		}
		protected override string OnGetDescription(string defaultLocalizationKey) {
			return "";
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			RegisterInitialProperty<IAccelerationForce>(new AccelerationForce());
			RegisterInitialProperty<IWalkSpeed>(new WalkSpeed());
			RegisterInitialProperty<ISprintSpeed>(new SprintSpeed());
			RegisterInitialProperty<ISlideSpeed>(new SlideSpeed());
			RegisterInitialProperty<IGroundDrag>(new GroundDrag());
			RegisterInitialProperty<IJumpForce>(new JumpForce());
			RegisterInitialProperty<IAdditionalGravity>(new AdditionalGravity());
			
			RegisterInitialProperty<IMaxSlideTime>(new MaxSlideTime());
			RegisterInitialProperty<ISlideForce>(new SlideForce());
			
			RegisterInitialProperty<IWallRunForce>(new WallRunForce());
		}


		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		public override void OnAwake() {
			base.OnAwake();
			accelerationForce = GetProperty<IAccelerationForce>();
			walkSpeed = GetProperty<IWalkSpeed>();
			sprintSpeed = GetProperty<ISprintSpeed>();
			slideSpeed = GetProperty<ISlideSpeed>();
			groundDrag = GetProperty<IGroundDrag>();
			jumpForce = GetProperty<IJumpForce>();
			additionalGravity = GetProperty<IAdditionalGravity>();
			maxSlideTime = GetProperty<IMaxSlideTime>();
			slideForce = GetProperty<ISlideForce>();
			wallRunForce = GetProperty<IWallRunForce>();
		}

		public IAccelerationForce GetAccelerationForce() {
			return accelerationForce;

		}
		
		public IWalkSpeed GetWalkSpeed() {
			return walkSpeed;
		}
		
		public ISprintSpeed GetSprintSpeed() {
			return sprintSpeed;
		}
		
		public ISlideSpeed GetSlideSpeed() {
			return slideSpeed;
		}
		
		public IGroundDrag GetGroundDrag() {
			return groundDrag;
		}
		
		public IJumpForce GetJumpForce() {
			return jumpForce;
		}
		
		public IAdditionalGravity GetAdditionalGravity() {
			return additionalGravity;
		}
		
		public IMaxSlideTime GetMaxSlideTime() {
			return maxSlideTime;
		}
		
		public ISlideForce GetSlideForce() {
			return slideForce;
		}
		
		public IWallRunForce GetWallRunForce() {
			return wallRunForce;
		}

		public MovementState GetMovementState()
		{
			return movementState;
		}

		public void SetMovementState(MovementState state)
		{
			movementState = state;
		}

		public bool IsScopedIn()
		{
			return scopedIn;
		}

		public void SetScopedIn(bool state)
		{
			scopedIn = state;
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}

		protected override Faction GetDefaultFaction() {
			return Faction.Friendly;
		}

		public void OnKillDamageable(IDamageable damageable) {
			
		}
	}
}