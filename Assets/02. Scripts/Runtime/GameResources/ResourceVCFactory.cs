﻿using System;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Currency.ViewControllers;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using Polyglot;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Model;
using UnityEngine;

namespace Runtime.GameResources {
	public class ResourceVCFactory : MikroSingleton<ResourceVCFactory>, ISingleton, ICanGetUtility, ICanGetSystem{

		private ResLoader resLoader;
		public override void OnSingletonInit() {
			base.OnSingletonInit();
			resLoader = this.GetUtility<ResLoader>();
		}

		private ResourceVCFactory() {
			
		}
		
		/// <summary>
		/// Spawn a pickable resource view controller (on ground) from a resource entity
		/// </summary>
		/// <param name="resourceEntity"></param>
		/// <param name="usePool"></param>
		/// <param name="poolInitCount"></param>
		/// <param name="poolMaxCount"></param>
		/// <returns></returns>
		public GameObject SpawnPickableResourceVC(IResourceEntity resourceEntity, bool usePool, 
			int poolInitCount = 5, int poolMaxCount = 20) {
			return SpawnResourceVC(resourceEntity, usePool, resourceEntity.OnGroundVCPrefabName, poolInitCount, poolMaxCount);
		}
		
		/// <summary>
		/// Spawn a resource view controller (in hand) from a resource entity
		/// </summary>
		/// <param name="resourceEntity"></param>
		/// <param name="usePool"></param>
		/// <param name="poolInitCount"></param>
		/// <param name="poolMaxCount"></param>
		/// <returns></returns>
		public GameObject SpawnInHandResourceVC(IResourceEntity resourceEntity, bool usePool, 
			int poolInitCount = 5, int poolMaxCount = 20) {
			if (String.IsNullOrEmpty(resourceEntity.InHandVCPrefabName)) {
				return null;
			}
			return SpawnResourceVC(resourceEntity, usePool, resourceEntity.InHandVCPrefabName, poolInitCount, poolMaxCount);
		}

		public static string GetLocalizedResourceCategory(ResourceCategory category) {
			return Localization.Get("NAME_" + category.ToString());
		}
		
		
		public GameObject SpawnNewPickableResourceVC(string prefabName, bool usePool, bool setRarity = false, int rarity = 1, int poolInitCount = 5,
			int poolMaxCount = 20) {

			GameObject vc = SpawnResourceGameObject(prefabName, usePool, poolInitCount, poolMaxCount);
			
			IPickableResourceViewController vcComponent = vc.GetComponent<IPickableResourceViewController>();
			vcComponent.InitWithID(vcComponent.OnBuildNewPickableResourceEntity(setRarity, rarity).UUID);
			return vc;
		}
		
		public IResourceEntity SpawnNewResourceEntity(string pickableResourcePrefabName, bool setRarity = false, int rarity = 1) {
			GameObject prefab = resLoader.LoadSync<GameObject>(pickableResourcePrefabName);
			IPickableResourceViewController vcComponent = prefab.GetComponent<IPickableResourceViewController>();
			return vcComponent.OnBuildNewPickableResourceEntity(setRarity, rarity);
		}
		
		public void AddToInventoryOrSpawnNewVc(string pickableResourcePrefabName, bool usePool, Vector3 position,
			bool setRarity = false, int rarity = 1, int poolInitCount = 5, int poolMaxCount = 20) {
			IInventorySystem inventorySystem = this.GetSystem<IInventorySystem>();

			IResourceEntity resourceEntity = SpawnNewResourceEntity(pickableResourcePrefabName, setRarity, rarity);
			if (inventorySystem.AddItem(resourceEntity)) {
				return;
			}

			GameObject vc = SpawnPickableResourceVC(resourceEntity, usePool, poolInitCount, poolMaxCount);
			vc.transform.position = position;
		}
		
		private GameObject SpawnResourceGameObject(string prefabName, bool usePool, int poolInitCount = 5,
			int poolMaxCount = 20) {

			GameObject vc = null;
			if (usePool) {
				SafeGameObjectPool pool = GameObjectPoolManager.Singleton.CreatePoolFromAB(
					prefabName, null,
					poolInitCount, poolMaxCount, out GameObject prefab);
				vc = pool.Allocate();
				vc.transform.position = Vector3.zero;
				vc.transform.localScale = Vector3.one;
				vc.transform.rotation = Quaternion.identity;
			}
			else {
				GameObject prefab = resLoader.LoadSync<GameObject>(prefabName);
				vc = GameObject.Instantiate(prefab);
			}
			
			return vc;
		}

		public GameObject SpawnPickableCurrency(CurrencyType currencyType, int amount) {
			GameObject vc = SpawnResourceGameObject($"{currencyType.ToString()}Currency", true);
			IPickableCurrencyViewController vcComponent = vc.GetComponent<IPickableCurrencyViewController>();
			vcComponent.InitWithID(vcComponent.OnBuildNewPickableCurrencyEntity(currencyType, amount).UUID);
			return vc;
		}
		
		public GameObject SpawnDeployableResourceVC(IResourceEntity resourceEntity, bool usePool, bool isPreview,
			int poolInitCount = 5, int poolMaxCount = 20) {
			GameObject spawnedVc = SpawnResourceVC(resourceEntity, usePool, resourceEntity.DeployedVCPrefabName, poolInitCount, poolMaxCount);
			spawnedVc.GetComponent<IDeployableResourceViewController>().SetPreview(isPreview);
			return spawnedVc;
		}

		private GameObject SpawnResourceVC(IResourceEntity resourceEntity, bool usePool, 
			string prefabName, int poolInitCount, int poolMaxCount) {
			GameObject vc = null;
			if (usePool) {
				SafeGameObjectPool pool = GameObjectPoolManager.Singleton.CreatePoolFromAB(
					prefabName, null,
					poolInitCount, poolMaxCount, out GameObject prefab);
				vc = pool.Allocate();
				vc.transform.position = Vector3.zero;
				vc.transform.localScale = Vector3.one;
				vc.transform.rotation = Quaternion.identity;
			}
			else {
				GameObject prefab = resLoader.LoadSync<GameObject>(prefabName);
				vc = GameObject.Instantiate(prefab);
			}
			
			IResourceViewController vcComponent = vc.GetComponent<IResourceViewController>();
			vcComponent.InitWithID(resourceEntity.UUID);
			return vc;
		}
		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}