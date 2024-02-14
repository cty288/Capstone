﻿using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.ResourceCrafting.Models;
using _02._Scripts.Runtime.ResourceCrafting.Models.Build;
using _02._Scripts.Runtime.WeaponParts.Model;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Systems {
	public interface IWeaponPartsSystem : ISystem {
		
	}
	public class WeaponPartsSystem : AbstractSystem, IWeaponPartsSystem{
		private IBuffSystem buffSystem;	
		private IWeaponPartsModel weaponPartsModel;

		private static string[] initiallyUnlockedPartsNames = new string[] {
			/*"SpecialBarrel",
			"HeavyBarrel",
			"AdaptedCompensator",
			"SpecialCompensator",
			"ShortBarrel",
			"LongBarrel",*/
			"SpecialBarrel",
			"CustomizedMagazine",
			"SelfLoadingAttachment",
			"FatesEdgeBarrel",
			"DoubleExplosionMagazine",
			"VerticalForegrip",
			"ThundercrackBarrel",
			"EMPChain",
			"IntenseEMP",
			"EvolvingVirus",
			"WorseViral",
			"Multivirus"
		};
		
		protected IResourceBuildModel buildModel;
		 
		
		protected override void OnInit() {
			buffSystem = this.GetSystem<IBuffSystem>();
			this.RegisterEvent<OnWeaponPartsUpdate>(OnWeaponPartsUpdate);
			weaponPartsModel = this.GetModel<IWeaponPartsModel>();
			
			buildModel = this.GetModel<IResourceBuildModel>();
			if (buildModel.IsFirstTimeCreated) {
				foreach (string skillName in initiallyUnlockedPartsNames) {
					//ISkillEntity skillEntity = GetNewSkillEntity(skillName);
					//buildModel.UnlockBuild(ResearchCategory.WeaponAndParts, skillName, false);
					weaponPartsModel.AddToUnlockedParts(skillName);
				}
			}
		}

		private void UpdateBuildBuff(IWeaponEntity weaponEntity) {
			if (weaponEntity.CurrentBuildBuffType != null) {
				buffSystem.RemoveBuff(weaponEntity, weaponEntity.CurrentBuildBuffType);
			}

			weaponEntity.CurrentBuildBuffType = null;

			CurrencyType newBuildType = weaponEntity.GetMainBuildType();
			int totalBuildRarity =weaponEntity.GetTotalBuildRarity(newBuildType);
			int buildBuffRarity = weaponEntity.GetBuildBuffRarityFromBuildTotalRarity(totalBuildRarity);
			Debug.Log("Weapon Build Update. Build Type: " + newBuildType + " Rarity: " + buildBuffRarity +
			          " Total Rarity: " + totalBuildRarity + " Weapon: " + weaponEntity.GetDisplayName());
			
			
			if (buildBuffRarity <= 0) {
				return;
			}

			
			
			BuffBuilder buffBuilder = BuffPool.GetWeaponBuildBuff(newBuildType);
			if (buffBuilder == null) {
				return;
			}
			IBuff buildBuff = buffBuilder(weaponEntity, weaponEntity, buildBuffRarity);
			if (!buffSystem.AddBuff(weaponEntity, weaponEntity, buildBuff)) {
				buildBuff.RecycleToCache();
			}else {
				weaponEntity.CurrentBuildBuffType = buildBuff.GetType();
			}
		}

		private void OnWeaponPartsUpdate(OnWeaponPartsUpdate e) {
			IWeaponPartsEntity previousWeaponParts =
				GlobalGameResourceEntities.GetAnyResource(e.PreviousTopPartsUUID) as IWeaponPartsEntity;
            
			if(previousWeaponParts != null) {
				buffSystem.RemoveBuff(e.WeaponEntity, previousWeaponParts.BuffType);
			}
			
			UpdateBuildBuff(e.WeaponEntity);
			
			
			IWeaponPartsEntity newWeaponParts =
				GlobalGameResourceEntities.GetAnyResource(e.CurrentTopPartsUUID) as IWeaponPartsEntity;
			if(newWeaponParts != null) {
				IBuff buff = newWeaponParts.OnGetBuff(e.WeaponEntity);
				if (!buffSystem.AddBuff(e.WeaponEntity, newWeaponParts, buff)) {
					buff.RecycleToCache();
				}
			}

			

		}
	}
}