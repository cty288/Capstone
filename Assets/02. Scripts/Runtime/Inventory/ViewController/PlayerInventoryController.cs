using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Commands;
using UnityEngine;

public class PlayerInventoryController : AbstractMikroController<MainGame> {
   [SerializeField] private GameObject throwPoint;
   [SerializeField] private float throwForce = 10f;
   private void Awake() {
      this.RegisterEvent<OnPlayerThrowResource>(OnPlayerThrowResource).UnRegisterWhenGameObjectDestroyed(gameObject);
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
