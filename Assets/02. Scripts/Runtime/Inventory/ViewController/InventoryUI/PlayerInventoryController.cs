using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using MikroFramework.Event;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using Runtime.Controls;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Commands;
using Runtime.Inventory.Model;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventoryController : AbstractMikroController<MainGame> {
   [SerializeField] private GameObject throwPoint;
   [SerializeField] private float throwForce = 10f;
   
   [SerializeField] private List<GameObject> initialItems;

   private DPunkInputs.SharedActions sharedActions;
   private IInventoryModel inventoryModel;
   
   private void Awake() {
      sharedActions = ClientInput.Singleton.GetSharedActions();
      this.RegisterEvent<OnPlayerThrowResource>(OnPlayerThrowResource).UnRegisterWhenGameObjectDestroyed(gameObject);
      inventoryModel = this.GetModel<IInventoryModel>();
   }

   private void Start() {
      if (inventoryModel.IsFirstTimeCreated) {
         AssignInitialItems();
      }
      
   }

   private void AssignInitialItems() {
      
      foreach (GameObject item in initialItems) {
         IPickableResourceViewController resourceViewController = item.GetComponent<IPickableResourceViewController>();
         IResourceEntity resourceEntity = resourceViewController.OnBuildNewPickableResourceEntity(false, 1);

         inventoryModel.AddItem(resourceEntity);
      }
   }


   private void Update() {
      //Alpha1 -> 49, Alpha9 -> 57
      //map Alpha1 -> 49 to index 0, Alpha9 -> 57 to index 8
      InputAction leftNavigate = sharedActions.HotBarLeftNavigate;
      float leftNavigation = leftNavigate.ReadValue<float>();
      if (leftNavigation != 0 && leftNavigate.WasPressedThisFrame()) {
         if(leftNavigation > 0) {
            //right
            this.SendCommand(NavigateSelectHotBarSlotCommand.Allocate(HotBarCategory.Left, true));
         }
         else {
            //left
            this.SendCommand(NavigateSelectHotBarSlotCommand.Allocate(HotBarCategory.Left, false));
         }
      }

      InputAction rightNavigate = sharedActions.HotBarRightNavigate;
      float rightNavigation = rightNavigate.ReadValue<float>();
      if (rightNavigation != 0 && rightNavigate.WasPressedThisFrame()) {
         if(rightNavigation > 0) {
            //right
            this.SendCommand(NavigateSelectHotBarSlotCommand.Allocate(HotBarCategory.Right, true));
         }
         else {
            //left
            this.SendCommand(NavigateSelectHotBarSlotCommand.Allocate(HotBarCategory.Right, false));
         }
      }
      
      
      for (int i = 0; i < 9; i++) {
         if (Input.GetKeyDown((KeyCode) (49 + i))) {
            this.SendCommand(DirectSelectHotBarSlotCommand.Allocate(HotBarCategory.Left, i));
            break;
         }
      }
      
   }

   private void OnPlayerThrowResource(OnPlayerThrowResource e) {
      List<IResourceEntity> resources = e.resources;
      foreach (IResourceEntity resourceEntity in resources) {
         GameObject resourceVC = ResourceVCFactory.Singleton.SpawnPickableResourceVC(resourceEntity, true);
         resourceVC.transform.position = throwPoint.transform.position;

         resourceVC.GetComponent<IPickableResourceViewController>().HoldAbsorb = true;
         //add 3d force
         Rigidbody rigidbody2D = resourceVC.GetComponent<Rigidbody>();
         //face toward the blue axis of throw point
         Vector3 direction = throwPoint.transform.forward;
         rigidbody2D.AddForce(direction * throwForce);
         
      }
   }
}
