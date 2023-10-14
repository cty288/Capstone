using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using UnityEngine;

namespace Runtime.GameResources {
	public class ResourceVCFactory : MikroSingleton<ResourceVCFactory>, ISingleton, ICanGetUtility{
		
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
			return SpawnResourceVC(resourceEntity, usePool, resourceEntity.InHandVCPrefabName, poolInitCount, poolMaxCount);
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