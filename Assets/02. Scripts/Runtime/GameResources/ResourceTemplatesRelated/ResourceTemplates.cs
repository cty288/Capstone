using System;
using System.Collections.Generic;
using System.Linq;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.DataStructures;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using UnityEngine;

namespace Runtime.GameResources {
	public delegate IResourceEntity ResourceEntityCreater(bool setRarity, int rarity);
	
	
	public class ResourceTemplateInfo {
		public IResourceEntity TemplateEntity { get; private set; }
		public ResourceEntityCreater EntityCreater { get; private set; }
		
		public ResourceTemplateInfo(IResourceEntity templateEntity, ResourceEntityCreater entityCreater) {
			TemplateEntity = templateEntity;
			EntityCreater = entityCreater;
		}
	}
	
	
	
	
	public class ResourceTemplates : MikroSingleton<ResourceTemplates>, ICanGetModel, ICanGetUtility {
		private ResLoader resLoader;

		private ResourceTemplateTable resourceTemplateTable = new ResourceTemplateTable();
		public override void OnSingletonInit() {
			base.OnSingletonInit();
			resLoader = this.GetUtility<ResLoader>();
			if (resourceTemplateTable.Items.Count == 0) {
				LoadResourceTemplates();
			}
		}
		
		private ResourceTemplates() {
			
		}
		
		public IEnumerable<ResourceTemplateInfo> GetResourceTemplates(Predicate<IResourceEntity> predicate) {
			return resourceTemplateTable.Items?.Where(info => predicate(info.TemplateEntity));
		}

		public IEnumerable<ResourceTemplateInfo> GetResourceTemplates(ResourceCategory category,
			Predicate<IResourceEntity> additionalPredicate = null) {
			return resourceTemplateTable.CategoryIndex.Get(category)?.Where(info =>
				additionalPredicate == null || additionalPredicate(info.TemplateEntity));
		}
		
		public ResourceTemplateInfo GetResourceTemplates(string entityName) {
			return resourceTemplateTable.NameIndex.Get(entityName)?.FirstOrDefault();
		}

		private void LoadResourceTemplates() {
			AssetBundleData abData = ResData.instance.GetAssetBundleDataFromABName("entities/resources");
			List<AssetData> prefabs = new List<AssetData>();
			foreach (AssetData data in abData.AssetDataList) {
				if (data.AssetType.Contains("GameObject")) {
					prefabs.Add(data);
				}
			}
			
			//load these prefabs
			foreach (AssetData data in prefabs) {
				GameObject prefab = resLoader.LoadSync<GameObject>(data.OwnerBundleName, data.Name);
				if (prefab.TryGetComponent<IPickableResourceViewController>(out var vc)) {
					IResourceEntity templateEntity = vc.OnBuildNewPickableResourceEntity(false, 1, false);
					if (!templateEntity.Collectable) {
						templateEntity.RecycleToCache();
						continue;
					}

					ResourceTemplateInfo info = new ResourceTemplateInfo(templateEntity,
						((setRarity, rarity) => vc.OnBuildNewPickableResourceEntity(setRarity, rarity)));

					resourceTemplateTable.Add(info);
				}
			}
			
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}