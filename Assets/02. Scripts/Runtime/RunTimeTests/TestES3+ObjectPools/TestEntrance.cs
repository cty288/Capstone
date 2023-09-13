using MikroFramework.ResKit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runtime.RunTimeTests.TestES3_ObjectPools {
    public class TestEntrance : MonoBehaviour {
        private ResLoader resLoader;

        private void Awake() {
            ResLoader.Create((loader => {
                resLoader = loader;
                OnResLoaderInit();
            }));
        }

        private void OnResLoaderInit() {
            resLoader.LoadSync<AssetBundle>("scene");
            SceneManager.LoadScene("SceneTestES3");
        }

        void Start() {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
