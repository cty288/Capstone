using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCameraMovement : MonoBehaviour
{
    	
    Camera cam;
	

    [SerializeField] private float lerpSpeed = 0.3f;
    [SerializeField] private bool noLerp = false;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    private void Awake() {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate() {
        if (!noLerp) {
            //lerp the position of the camera to the player's position
            transform.position = Vector3.Lerp(transform.position, target.transform.position + offset, lerpSpeed * Time.deltaTime);
		
            //lerp the rotation of the camera to the player's rotation
            transform.rotation = Quaternion.Lerp(transform.rotation, target.transform.rotation, lerpSpeed * Time.deltaTime);
        }
        else {
            transform.position = target.transform.position + offset;
            transform.rotation = target.transform.rotation;
        }
		
		
    }
}
