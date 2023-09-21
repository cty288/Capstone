using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.ResKit;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

public class GameEntrance : MonoBehaviour
{
    private void Awake() {
        //load necessary resources before entering the game
        ConfigDatas.Singleton.OnSingletonInit();
        ResLoader.Create((loader) => {
            //load next scene in build settings
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        });
    }
}
