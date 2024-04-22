using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.AudioKit;
using MikroFramework.ResKit;
using Polyglot;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

public class GameEntrance : MonoBehaviour
{
    private void Awake() {
        //load necessary resources before entering the game
        ConfigDatas.Singleton.OnSingletonInit();
        AudioSystem.Singleton.Initialize(null);
        LoadSettings();
        ResLoader.Create((loader) => {
            //load next scene in build settings
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        });
    }

    private void LoadSettings() {
        Mikrocosmos.Resolution currentResolution = ES3.Load<Mikrocosmos.Resolution>("resolution", new Mikrocosmos.Resolution() { width = 1920, height = 1080 });
        bool isFullScreen = ES3.Load<bool>("fullScreen", false);
        Screen.SetResolution(currentResolution.width, currentResolution.height, isFullScreen);
        QualitySettings.SetQualityLevel(ES3.Load<int>("quality_level", 2));

        Localization.Instance.SelectLanguage(ES3.Load<Language>("language",
            Localization.Instance.ConvertSystemLanguage(Application.systemLanguage)));
    }
}
