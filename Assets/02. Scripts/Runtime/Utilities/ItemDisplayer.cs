using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.ResKit;
using UnityEngine;

public class ItemDisplayer : MonoBehaviour
{
    private Camera displayCamera;
    private Transform spawnPos;
    private float rotationSpeed = 10f;
    private static int displayedObjectCount = 0;
    private static float increaseSteps = 100;
    
    private static Vector3 originalSpawnPos = new Vector3(5000, 5000, 5000);
    
    private void Awake() {
        displayCamera = transform.Find("ModelDisplayCamera").GetComponent<Camera>();
        spawnPos = transform.Find("SpawnPos");
    }

    private void Update() {
        spawnPos.Rotate(Vector3.up, rotationSpeed * Time.unscaledDeltaTime);
    }
    
    

    public static ItemDisplayer Create(GameObject displayedObjectPrefab, RenderTexture outputTexture,
        float rotationSpeed = 10f, float fov = 20) {

        ResLoader resLoader = MainGame.Interface.GetUtility<ResLoader>();
        GameObject itemDisplayerPrefab = resLoader.LoadSync<GameObject>("ItemDisplayer");

        Vector3 spawnPos = originalSpawnPos + Vector3.right * displayedObjectCount * increaseSteps;

        GameObject itemDisplayerObj = Instantiate(itemDisplayerPrefab, spawnPos, Quaternion.identity);
        
        ItemDisplayer itemDisplayer = itemDisplayerObj.GetComponent<ItemDisplayer>();
        itemDisplayer.displayCamera.targetTexture = outputTexture;
        itemDisplayer.rotationSpeed = rotationSpeed;
        itemDisplayer.displayCamera.fieldOfView = fov;
        
        GameObject displayedObject = Instantiate(displayedObjectPrefab, itemDisplayer.spawnPos);
        displayedObject.transform.localPosition = Vector3.zero;
        displayedObject.transform.localRotation = Quaternion.identity;
        
        
        displayedObjectCount++;
        
        return itemDisplayer;
    }

    public void DestroyDisplayer() {
        displayedObjectCount--;
        Destroy(gameObject);
    }
}
