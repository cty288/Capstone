using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using UnityEngine;

namespace Runtime.Enemies {
	public class EnemyVCFactory: MikroSingleton<EnemyVCFactory>, ISingleton, ICanGetUtility {
		
		private ResLoader resLoader;
		public override void OnSingletonInit() {
			base.OnSingletonInit();
			resLoader = this.GetUtility<ResLoader>();
		}

		private EnemyVCFactory() {
			
		}
		

		public GameObject SpawnEnemyVC(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, int rarity, int level, bool usePool, int poolInitCount, int poolMaxCount) {
			IEnemyViewController prefabComponent = prefab.GetComponent<IEnemyViewController>();
			IEnemyEntity entity = prefabComponent.OnInitEntity(level, rarity);
				
				
			GameObject vc = null;
			if (usePool) {
				SafeGameObjectPool pool = GameObjectPoolManager.Singleton.CreatePool(
					prefab,
					poolInitCount, poolMaxCount);
				vc = pool.Allocate();
				vc.transform.position = position;
				vc.transform.rotation = rotation;
				vc.transform.SetParent(parent);
				
			}
			else {
				vc = GameObject.Instantiate(prefab);
			}
			
			IEnemyViewController vcComponent = vc.GetComponent<IEnemyViewController>();
			vcComponent.InitWithID(entity.UUID);
			
			return vc;
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}