using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.ResKit;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

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
