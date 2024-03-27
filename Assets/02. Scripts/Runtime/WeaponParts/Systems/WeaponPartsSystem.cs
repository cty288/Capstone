using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.ResourceCrafting.Models;
using _02._Scripts.Runtime.ResourceCrafting.Models.Build;
using _02._Scripts.Runtime.WeaponParts.Model;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Systems {
	public interface IWeaponPartsSystem : ISystem {
		public HashSet<IWeaponPartsEntity> GetCurrentLevelPurchaseableParts();
		
		public void RemoveCurrentLevelPurchaseableParts(IWeaponPartsEntity resourceEntity);
	}
	public class WeaponPartsSystem : AbstractSystem, IWeaponPartsSystem{
		private IBuffSystem buffSystem;	
		private IWeaponPartsModel weaponPartsModel;
		private  ILevelModel levelModel;

		private static string[] initiallyUnlockedPartsNames = new string[] {
			"ShreddingAdaptedBarrel", "ShreddingAdaptedMagazine", "ShreddingAdaptedAttachment", "ExecutorBarrel",
			"ShrapnelBullets", "EMPAdaptedBarrel", "EMPAdaptedMagazine", "EMPAdaptedAttachment", "SpecialCompensator",
			"InterferenceAmplification", "ViralAdaptedBarrel", "ViralAdaptedMagazine", "ViralAdaptedAttachment",
			"HardCasesBarrel", "EvolvingVirus", "SpecialBarrel", "HeavyBarrel", "GunpowerEnchancement",
			"ArtificialAdaptedMagazine", "ArtificialAdaptedAttachment"
		};
		
		protected IResourceBuildModel buildModel;
		 
		private HashSet<IWeaponPartsEntity> currentLevelPurchaseableParts = new HashSet<IWeaponPartsEntity>();

		protected override void OnInit() {
			buffSystem = this.GetSystem<IBuffSystem>();
			this.RegisterEvent<OnEquippedWeaponPartsUpdate>(OnWeaponPartsUpdate);
			weaponPartsModel = this.GetModel<IWeaponPartsModel>();
			levelModel = this.GetModel<ILevelModel>();
			
			buildModel = this.GetModel<IResourceBuildModel>();
			if (buildModel.IsFirstTimeCreated) {
				/*buildModel.UnlockBuild(ResearchCategory.WeaponAndParts, "Shotgun", false);
				buildModel.UnlockBuild(ResearchCategory.WeaponAndParts, "Multivirus", false);
				buildModel.UnlockBuild(ResearchCategory.WeaponAndParts, "FatesEdgeBarrel", false);
				buildModel.UnlockBuild(ResearchCategory.WeaponAndParts, "LongBarrel", false);
				buildModel.UnlockBuild(ResearchCategory.Skill, "AdrenalineSkill", false);
				buildModel.UnlockBuild(ResearchCategory.Skill, "TenaciousBodySkill", false);
				buildModel.UnlockBuild(ResearchCategory.Skill, "HighEndTechnologySkill", false);
				buildModel.UnlockBuild(ResearchCategory.Skill, "AdrenalineSkill", false);
				buildModel.UnlockBuild(ResearchCategory.Skill, "TurretSkill", false);
				buildModel.UnlockBuild(ResearchCategory.Skill, "MedicalNeedleSkill", false);*/
				foreach (string skillName in initiallyUnlockedPartsNames) {
					//ISkillEntity skillEntity = GetNewSkillEntity(skillName);
					//buildModel.UnlockBuild(ResearchCategory.WeaponAndParts, skillName, false);
					weaponPartsModel.AddToUnlockedParts(skillName);
					var template = ResourceTemplates.Singleton.GetResourceTemplates(skillName);
					if (template == null) {
						Debug.LogError("Template not found for " + skillName);
						continue;
					}
				}
			}
			levelModel.CurrentLevelCount.RegisterWithInitValue(OnLevelUpdate);
			
		}

		private void OnLevelUpdate(int oldLevel, int newLevel) {
			if (newLevel < 1 || newLevel > LevelModel.MAX_LEVEL) {
				return;
			}
			
			RemoveCurrentLevelPurchaseableParts();
			GenerateCurrentLevelPurchaseableParts(newLevel);

		}

		private void GenerateCurrentLevelPurchaseableParts(int newLevel) {
			//generate new in-game purchasable parts
			List<(ResourceTemplateInfo, int)> selectedWeaponParts = new List<(ResourceTemplateInfo, int)>();
			HashSet<string> alreadySelectedWeaponParts = new HashSet<string>();
			
			for (int i = 0; i < 5; i++) {
				int level = Random.Range(newLevel, newLevel + 2);
				level = Mathf.Clamp(level, 1, LevelModel.MAX_LEVEL);
				
				var weaponParts =
					ResourceTemplates.Singleton.GetResourceTemplates(ResourceCategory.WeaponParts,
						(parts) => {
							IWeaponPartsEntity template = (IWeaponPartsEntity) parts;
							return weaponPartsModel.IsUnlocked(parts.EntityName) &&
							       template.GetMaxRarity() >= level &&
							       template.GetMinRarity() <= level &&
							       !alreadySelectedWeaponParts.Contains(parts.EntityName);
						});

				var weaponPartTemplateInfos = weaponParts.ToList();
				if (weaponParts == null || !weaponPartTemplateInfos.Any()) {
					break;
				}

							
				if (weaponPartTemplateInfos.Count == 0) {
					break;
				}
						
				int randomIndex = Random.Range(0, weaponPartTemplateInfos.Count);
				ResourceTemplateInfo selectedWeaponPart = weaponPartTemplateInfos[randomIndex];
				
				selectedWeaponParts.Add((selectedWeaponPart, level));
				alreadySelectedWeaponParts.Add(selectedWeaponPart.TemplateEntity.EntityName);
			}
			
			
			foreach (var weaponPartTuple in selectedWeaponParts) {
				ResourceTemplateInfo weaponPart = weaponPartTuple.Item1;
				int level = weaponPartTuple.Item2;
				IResourceEntity resource =
					weaponPart.EntityCreater.Invoke(true, level);
				
				currentLevelPurchaseableParts.Add(resource as IWeaponPartsEntity);
			}
		}

		private void RemoveCurrentLevelPurchaseableParts() {
			foreach (IResourceEntity resourceEntity in currentLevelPurchaseableParts) {
				GlobalEntities.GetEntityAndModel(resourceEntity.UUID).Item2.RemoveEntity(resourceEntity.UUID);
			}
			currentLevelPurchaseableParts.Clear();
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

		private void OnWeaponPartsUpdate(OnEquippedWeaponPartsUpdate e) {
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

		public HashSet<IWeaponPartsEntity> GetCurrentLevelPurchaseableParts() {
			return new HashSet<IWeaponPartsEntity>(currentLevelPurchaseableParts);
		}

		public void RemoveCurrentLevelPurchaseableParts(IWeaponPartsEntity resourceEntity) {
			currentLevelPurchaseableParts.Remove(resourceEntity);
		}
	}
}