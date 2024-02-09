using _02._Scripts.Runtime.BuffSystem.ConfigurableBuff;
using Framework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.Weapons.Model.Base;

namespace _02._Scripts.Runtime.WeaponParts.Model.Base {
	public interface IWeaponBuildBuff : ILeveledBuff {
		string[] GetAllLevelDescriptions();
	}

	public class WeaponBuildBuffEvent {
		public IWeaponEntity WeaponEntity { get; set; }
	}
	

	
	
	
	
	
	public abstract class WeaponBuildBuff<T> : ConfigurableBuff<T>, IWeaponBuildBuff, ICanSendEvent where T : WeaponBuildBuff<T>, new(){
		[field: ES3Serializable]
		public override float MaxDuration { get; protected set; } = -1;
		public override int Priority { get; } = 1;
		
		protected IWeaponEntity weaponEntity;
		
		public override string OnGetDescription(string defaultLocalizationKey) {
			return null;
		}

		public override bool IsDisplayed() {
			return false;
		}

		public override bool Validate() {
			return base.Validate() && buffOwner is IWeaponEntity && OnValidate();
		}

		protected abstract bool OnValidate();

		public override bool IsGoodBuff => true;
		
		public abstract string[] GetAllLevelDescriptions();

		public override string GetLevelDescription(int level) {
			int index = level - 1;
			string[] allLevelDescriptions = GetAllLevelDescriptions();
			if (index < 0 || index >= allLevelDescriptions.Length) {
				return string.Empty;
			}

			return allLevelDescriptions[index];
		}
		
		

		protected override void OnLevelUp() {
			
		}

		public override void OnInitialize(IEntity buffDealer, IEntity entity, bool force = false) {
			weaponEntity = entity as IWeaponEntity;
			base.OnInitialize(buffDealer, entity, force);
		}

		protected override void OnBuffStacked(T buff) {
			
		}
		
		public void SendWeaponBuildBuffEvent<T>(T weaponBuildBuffEvent) where T : WeaponBuildBuffEvent  {
			weaponBuildBuffEvent.WeaponEntity = weaponEntity;
			this.SendEvent<T>(weaponBuildBuffEvent);
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}