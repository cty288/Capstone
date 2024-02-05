using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.WeaponParts.Model;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Cysharp.Threading.Tasks;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Singletons;
using MikroFramework.UIKit;
using Runtime.DataFramework.Properties;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.RawMaterials.Model.Base;
using Runtime.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _02._Scripts.Runtime.Rewards {
	public class RewardDisperser : MikroSingleton<RewardDisperser>, IController {
		private RewardDisperser() {
			
		}
		
		/// <summary>
		/// Will disperse the rewards given in the list of RewardBatches.
		/// This is an async method, because some rewards may need UI selection by the player.
		/// The callback will be called when all rewards are dispersed, including all spawned GameObjects
		/// </summary>
		/// <param name="rewardBatches"></param>
		/// <param name="onRewardsDispersed"></param>
		public async UniTask<List<GameObject>> DisperseRewards(List<RewardBatch> rewardBatches, IPanelContainer parentPanel,
			CurrencyType buildType) {
			
			List<GameObject> spawnedGameObjects = new List<GameObject>();
			
			foreach (var rewardBatch in rewardBatches) {
				List<GameObject> spawnedGameObjectsInBatch = await DisperseRewards(rewardBatch, parentPanel, buildType);
				if (spawnedGameObjectsInBatch != null) {
					spawnedGameObjects.AddRange(spawnedGameObjectsInBatch);
				}
			}

			return spawnedGameObjects;
		}


		
		private async UniTask<List<GameObject>> DisperseRewards(RewardBatch rewardBatch, IPanelContainer parentPanel,
			CurrencyType buildType) {
			
			int count = Random.Range(rewardBatch.AmountRange.x, rewardBatch.AmountRange.y + 1);
			List<GameObject> spawnedGameObjects = new List<GameObject>();
			switch (rewardBatch.RewardType) {
				case RewardType.Resource:
					
					var resources = 
						ResourceTemplates.Singleton.GetResourceTemplates(ResourceCategory.RawMaterial, entity =>
						((IRawMaterialEntity) entity).GetProperty<IRarityProperty>().RealValue.Value ==
						rewardBatch.Level);

					var resourceTemplateInfos = resources as ResourceTemplateInfo[] ?? resources.ToArray();
					if (resources == null || !resourceTemplateInfos.Any()) {
						return null;
					}

					var targetResource = resourceTemplateInfos[Random.Range(0, resourceTemplateInfos.Length)];
					
					for (int i = 0; i < count; i++) {
						IResourceEntity resource = targetResource.EntityCreater.Invoke(false, rewardBatch.Level);
						GameObject spawnedVC = ResourceVCFactory.Singleton.SpawnPickableResourceVC(resource, true);
						spawnedGameObjects.Add(spawnedVC);
					}
					return spawnedGameObjects;
					break;
				
				case RewardType.WeaponParts_ChooseOne:
					RewardSelectionPanel panel = null;
					IWeaponPartsModel weaponPartsModel = this.GetModel<IWeaponPartsModel>();
					
					for (int m = 0; m < count; m++) {
						var weaponParts =
							ResourceTemplates.Singleton.GetResourceTemplates(ResourceCategory.WeaponParts,
								(parts) => {
									IWeaponPartsEntity template = (IWeaponPartsEntity) parts;
									return weaponPartsModel.IsUnlocked(parts.EntityName) &&
									       template.GetMaxRarity() >= rewardBatch.Level &&
									       template.GetMinRarity() <= rewardBatch.Level &&
									       template.GetBuildType() == buildType;
								});

						var weaponPartTemplateInfos = weaponParts.ToList();
						if (weaponParts == null || !weaponPartTemplateInfos.Any()) {
							return null;
						}
					
					
						//select 3 different weapon parts
						List<ResourceTemplateInfo> selectedWeaponParts = new List<ResourceTemplateInfo>();
						for (int i = 0; i < 3; i++) {
							if (weaponPartTemplateInfos.Count == 0) {
								break;
							}
						
							int randomIndex = Random.Range(0, weaponPartTemplateInfos.Count);
							ResourceTemplateInfo selectedWeaponPart = weaponPartTemplateInfos[randomIndex];
							selectedWeaponParts.Add(selectedWeaponPart);
							weaponPartTemplateInfos.RemoveAt(randomIndex);
						}
					
						List<IResourceEntity> spawnedWeaponParts = new List<IResourceEntity>();
						foreach (var weaponPart in selectedWeaponParts) {
							
							IResourceEntity resource =
								weaponPart.EntityCreater.Invoke(true, rewardBatch.Level);
							
							spawnedWeaponParts.Add(resource);
						}

						panel =
							MainUI.Singleton.Open<RewardSelectionPanel>(parentPanel, null, true);
						await UniTask.WaitForSeconds(0.1f, true);
						IResourceEntity selectedWeaponPartEntity = await panel.SelectReward(spawnedWeaponParts);
						MainUI.Singleton.GetAndClose(panel);
						
						GameObject spawnedVC = ResourceVCFactory.Singleton.SpawnPickableResourceVC(selectedWeaponPartEntity, true);
						spawnedGameObjects.Add(spawnedVC);
					}
					break;
			}
			
			return spawnedGameObjects;
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}