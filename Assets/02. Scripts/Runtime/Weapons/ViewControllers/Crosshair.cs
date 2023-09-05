using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Singletons;
using Runtime.DataFramework.ViewControllers.Entities;
using UnityEngine;
using Object = UnityEngine.Object;

public class Crosshair : MonoMikroSingleton<Crosshair> {
    private Transform centerTransform;
    private Camera mainCamera;   
    private float rayDistance = 100f;
    private IEntityViewController currentPointedEntity;
    private LayerMask detectLayerMask;

    public IEntityViewController CurrentPointedEntity => currentPointedEntity;
    
    private void Awake() {
        centerTransform = transform.Find("Center");
        mainCamera = Camera.main;
        detectLayerMask = LayerMask.GetMask("CrossHairDetect");
    }
    
    private void Update()
    {
        if (!mainCamera) {
            return;
        }
        Ray ray = mainCamera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        RaycastHit hit;
        bool hitEntity = false;

        if (Physics.Raycast(ray, out hit, rayDistance, detectLayerMask)) {
            if(hit.collider.transform.parent.TryGetComponent<IEntityViewController>(out var entityViewController)) {
                if (currentPointedEntity != null && currentPointedEntity != entityViewController) {
                    currentPointedEntity.OnUnPointByCrosshair();
                    currentPointedEntity = null;
                }

                if (currentPointedEntity == null) {
                    currentPointedEntity = entityViewController;
                    entityViewController.OnPointByCrosshair();
                }
                hitEntity = true;
            }
        }

        if (!hitEntity) {
            if (currentPointedEntity as Object != null) {
                currentPointedEntity.OnUnPointByCrosshair();
                currentPointedEntity = null;
            }
        }
    }
}
