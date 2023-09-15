using System.Collections.Generic;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using UnityEngine;

namespace Runtime.RunTimeTests.TestPropertyBehaviorTree {
    public class TestBehaviorTreePool : MonoBehaviour {
        [SerializeField] private GameObject prefab;
        private SafeGameObjectPool pool;
        private List<GameObject> cubes = new List<GameObject>();
        private void Awake() {
            pool = GameObjectPoolManager.Singleton.CreatePool(prefab, 10, 50);
        }
	

        private void Update() {
            if (Input.GetKeyDown(KeyCode.S)) {
                ((MainGame) MainGame.Interface).SaveGame();
            }


            if (Input.GetKeyDown(KeyCode.P)) {
                GameObject obj = pool.Allocate();
                cubes.Add(obj);
                obj.transform.position = Random.insideUnitSphere * 10;
            }

            if (Input.GetKeyDown(KeyCode.M)) {
                if (cubes.Count > 0) {
                    pool.Recycle(cubes[0]);
                    cubes.RemoveAt(0);
                }
            }
        
        }

        public IArchitecture GetArchitecture() {
            return MainGame.Interface;
        }
    }
}
