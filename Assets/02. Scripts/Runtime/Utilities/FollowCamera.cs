using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FollowCamera : MonoBehaviour {
	
	Camera mainCamera;
	

	[SerializeField] private float lerpSpeed = 0.3f;
	private CinemachineBrain cinemachineBrain;
	[SerializeField] private bool noLerp = false;

	private void Awake() {
		mainCamera = Camera.main;
		cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
	}

	private void LateUpdate() {
		cinemachineBrain.ManualUpdate();
		if (!noLerp) {
			//lerp the position of the camera to the player's position
			transform.position = Vector3.Lerp(transform.position, mainCamera.transform.position, lerpSpeed * Time.deltaTime);
		
			//lerp the rotation of the camera to the player's rotation
			transform.rotation = Quaternion.Lerp(transform.rotation, mainCamera.transform.rotation, lerpSpeed * Time.deltaTime);
		}
		else {
			transform.position = mainCamera.transform.position;
			transform.rotation = mainCamera.transform.rotation;
		}
		
		
	}
}
