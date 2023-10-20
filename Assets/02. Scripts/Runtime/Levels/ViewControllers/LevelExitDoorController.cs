using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Systems;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Polyglot;
using Runtime.UI.NameTags;
using UnityEngine;

public class LevelExitDoorController : AbstractMikroController<MainGame> {
    private GameObject exitDoorGameObject;
    private ILevelSystem levelSystem;
    private ILevelModel levelModel;
    private Transform hudSpawnPoint;
    private INameTag spawnedNameTag;

    private void Awake() {
        exitDoorGameObject = transform.Find("ExitDoor").gameObject;
        hudSpawnPoint = transform.Find("HUDSpawnPoint");
        exitDoorGameObject.SetActive(false);
        levelSystem = this.GetSystem<ILevelSystem>();
        levelModel = this.GetModel<ILevelModel>();
        spawnedNameTag = HUDManager.Singleton.SpawnHUDElement(hudSpawnPoint, "NameTag_General", HUDCategory.Exit, true)
            .GetComponent<INameTag>();
        
        
        levelSystem.IsLevelExitSatisfied.RegisterWithInitValue(OnLevelExitSatisfied)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        levelModel.CurrentLevel.Value.IsInBossFight.RegisterOnValueChanged(OnBossFightChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);


      

    }

    private void OnBossFightChanged(bool oldVal, bool newVal) {
        bool levelExitSatisfied = levelSystem.IsLevelExitSatisfied.Value;
        if(!newVal && levelExitSatisfied) {
            exitDoorGameObject.SetActive(true);
            SetExitDoorName(true, "EXIT_DOOR_STATE_1");
        }
        else {
            string localizationName = "EXIT_DOOR_STATE_2";
            if (!levelExitSatisfied) {
                localizationName = "EXIT_DOOR_STATE_3";
            }
            exitDoorGameObject.SetActive(false);
            SetExitDoorName(false, localizationName);
        }
    }

    private void SetExitDoorName(bool isOpen, string localizationName) {
        spawnedNameTag.SetName(Localization.Get(localizationName));
        
        
        if (isOpen) {
            //spawnedNameTag.SetName("Enter Next Level \n(will return to the Base in this version)");
        }
        else {
            //spawnedNameTag.SetName("Exit : Deactivated");
        }
    }

    private void OnLevelExitSatisfied(bool oldVal, bool newVal) {
        bool isInBossFight = levelModel.CurrentLevel.Value.IsInBossFight.Value;
        if(!isInBossFight && newVal) {
            exitDoorGameObject.SetActive(true);
            SetExitDoorName(true, "EXIT_DOOR_STATE_1");
        }
        else {
            string localizationName = "EXIT_DOOR_STATE_2";

            if (!newVal) {
                localizationName = "EXIT_DOOR_STATE_3";
            }
            
            exitDoorGameObject.SetActive(false);
            SetExitDoorName(false, localizationName);
        }
    }

    private void OnDestroy() {
        HUDManager.Singleton.DespawnHUDElement(hudSpawnPoint, HUDCategory.Exit);
    }
}
