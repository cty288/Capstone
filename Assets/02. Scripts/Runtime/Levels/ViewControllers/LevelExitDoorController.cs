using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Systems;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using MikroFramework.Event;
using Polyglot;
using Runtime.UI.NameTags;
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
    
    [SerializeField] private Animator animator;
    private static readonly int lower = Animator.StringToHash("Lower");
    private static readonly int raise = Animator.StringToHash("Raise");

    private void Awake() {
        // exitDoorGameObject = transform.Find("ExitDoor").gameObject;
        // hudSpawnPoint = transform.Find("HUDSpawnPoint");
        exitDoorGameObject.SetActive(false);
        levelSystem = this.GetSystem<ILevelSystem>();
        levelModel = this.GetModel<ILevelModel>();
        spawnedNameTagGameObject =
            HUDManager.Singleton.SpawnHUDElement(hudSpawnPoint, "NameTag_General", HUDCategory.Exit, true);
        
        spawnedNameTag = spawnedNameTagGameObject.GetComponent<INameTag>();
        
        
        levelSystem.IsLevelExitSatisfied.RegisterWithInitValue(OnLevelExitSatisfied)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
      


       spawnedNameTagGameObject.SetActive(false);

    }

    private void Start() {
        LowerDoor();
    }

    private void LowerDoor()
    {
        animator.SetTrigger(lower);
    }
    
    private void RaiseDoor()
    {
        animator.SetTrigger(raise);
    }
    
    public void PlayRiseAudio()
    {
        AudioSystem.Singleton.Play3DSound("door_rise", transform.position);
    }

    public void PlayLowerAudio()
    {
        AudioSystem.Singleton.Play3DSound("door_lower", transform.position);
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

        if (alwaysOpen) {
            newVal = true;
        }
        
        if(newVal) { 
            RaiseDoor();
            exitDoorGameObject.SetActive(true);
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
