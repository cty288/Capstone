using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    Camera mainCamera;
    
    private void Awake() {
        mainCamera = Camera.main;
       
    }

    private void LateUpdate() {
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }
}
