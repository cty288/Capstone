using _02._Scripts.Runtime.BuffSystem.ConfigurableBuff;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.VFX;

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
		
		public static void SendWeaponBuildBuffEvent<T>(IWeaponEntity weaponEntity, T weaponBuildBuffEvent) where T : WeaponBuildBuffEvent {
			weaponBuildBuffEvent.WeaponEntity = weaponEntity;
			MainGame.Interface.SendEvent<T>(weaponBuildBuffEvent);
		}
		
		protected SafeGameObjectPool bulletInVFXPool;
		protected SafeGameObjectPool bulletOutVFXPool;
		protected SafeGameObjectPool bulletHitVFXPool;
		protected GameObject pooledBulletIn;
		protected GameObject pooledBulletOut;
		protected GameObject pooledBulletHit;
		
		private IWeaponVFX _weaponVFX;
		private IHitScanWeaponVFX _hitScanWeaponVFX;
		private bool allocated = false;
		public void AllocateBuffVFX(IWeaponVFX weaponVFX, IHitScanWeaponVFX hitScanWeaponVFX)
		{
			if (allocated)
			{
				return;
			}

			if (weaponVFX == null || hitScanWeaponVFX == null)
			{
				return;
			}
			
			pooledBulletIn = bulletInVFXPool.Allocate();
			pooledBulletOut = bulletOutVFXPool.Allocate();
			pooledBulletHit = bulletHitVFXPool.Allocate();
			_weaponVFX = weaponVFX;
			_hitScanWeaponVFX = hitScanWeaponVFX;
			_weaponVFX.SetVFX(pooledBulletHit.GetComponent<VisualEffect>());
			_hitScanWeaponVFX.SetBulletVFX(new[]{pooledBulletIn.GetComponent<VisualEffect>()}, 
				new[]{pooledBulletOut.GetComponent<VisualEffect>()});
			allocated = true;
		}

		public void DeallocateBuffVFX()
		{
			allocated = false;
			_weaponVFX.ResetVFX();
			_hitScanWeaponVFX.ResetBulletVFX();
			
			pooledBulletIn.transform.parent = bulletInVFXPool.transform;
			bulletInVFXPool.Recycle(pooledBulletIn);
			
			pooledBulletOut.transform.parent = bulletOutVFXPool.transform;
			bulletOutVFXPool.Recycle(pooledBulletOut);

			pooledBulletHit.transform.parent = bulletHitVFXPool.transform;
			bulletHitVFXPool.Recycle(pooledBulletHit);
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}