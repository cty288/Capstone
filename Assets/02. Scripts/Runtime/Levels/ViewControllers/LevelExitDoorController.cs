using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Commands;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Systems;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using MikroFramework.Event;
using Polyglot;
using Runtime.UI.NameTags;
using Runtime.Utilities;
using Runtime.Weapons.ViewControllers.CrossHairs;
using UnityEngine;

public class LevelExitDoorController : AbstractMikroController<MainGame>, ICrossHairDetectable {
    [SerializeField] private GameObject exitDoorGameObject;
    private ILevelSystem levelSystem;
    private ILevelModel levelModel;
    [SerializeField] private Transform hudSpawnPoint;
    private INameTag spawnedNameTag;
    private GameObject spawnedNameTagGameObject;
    [SerializeField] private bool alwaysOpen = false;

    [SerializeField] private BoxCollider SpawnSizeCollider;
    public Transform playerSpawnPoint;
    
    [SerializeField] private Animator animator;
    private static readonly int lower = Animator.StringToHash("Lower");
    private static readonly int raise = Animator.StringToHash("Raise");

    private void Awake() {
        levelSystem = this.GetSystem<ILevelSystem>();
        levelModel = this.GetModel<ILevelModel>();
        
        spawnedNameTagGameObject =
            HUDManager.Singleton.SpawnHUDElement(hudSpawnPoint, "NameTag_General", HUDCategory.Exit, true);
        spawnedNameTag = spawnedNameTagGameObject.GetComponent<INameTag>();
        spawnedNameTagGameObject.SetActive(false);
        
        levelSystem.IsLevelExitSatisfied.RegisterWithInitValue(OnLevelExitSatisfied)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        this.RegisterEvent<OnLoadingScreenHide>(LowerDoor).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
    }

    private void LowerDoor(OnLoadingScreenHide e)
    {
        animator.SetTrigger(lower);
        print("LOWER DOOR");
    }
    
    private void RaiseDoor()
    {
        animator.SetTrigger(raise);
        print("RAISE DOOR");
    }
    
    public void PlayRiseAudio()
    {
        var source = AudioSystem.Singleton.Play3DSound("door_rise", transform.position);
        source.spatialBlend = 0.5f;
    }

    public void PlayLowerAudio()
    {
        var source = AudioSystem.Singleton.Play3DSound("door_lower", transform.position);
        source.spatialBlend = 0.5f;
    }
    
    private void SetExitDoorName(bool isOpen, string localizationName) {
        spawnedNameTag.SetName(Localization.Get(localizationName));
        
        // if (isOpen) {
            //spawnedNameTag.SetName("Enter Next Level \n(will return to the Base in this version)");
        // }
        // else {
            //spawnedNameTag.SetName("Exit : Deactivated");
        // }
    }

    private void OnLevelExitSatisfied(bool oldVal, bool newVal) {

        if (alwaysOpen) {
            newVal = true;
        }
        
        if(newVal) { 
            RaiseDoor();
            // exitDoorGameObject.SetActive(true);
            SetExitDoorName(true, "EXIT_DOOR_STATE_1");
        }
        else {
            string localizationName = "EXIT_DOOR_STATE_3";
            exitDoorGameObject.SetActive(false);
            SetExitDoorName(false, localizationName);
        }
    }

    private void OnDestroy() {
        HUDManager.Singleton.DespawnHUDElement(hudSpawnPoint, HUDCategory.Exit);
    }

    public void OnUnPointByCrosshair() {
        spawnedNameTagGameObject.SetActive(false);
    }

    public void OnPointByCrosshair() {
        spawnedNameTagGameObject.SetActive(true);
        spawnedNameTag.Refresh();
        StartCoroutine(RebuildLayout());
    }
    
    private IEnumerator RebuildLayout() {
        spawnedNameTag.Refresh();
        yield return null;
        spawnedNameTag.Refresh();
    }
}
