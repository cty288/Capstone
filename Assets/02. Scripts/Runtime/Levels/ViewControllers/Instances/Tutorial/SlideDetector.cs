using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Controls;
using UnityEngine;

public class SlideDetector : MonoBehaviour {
    [SerializeField] private GameObject barrier;
    [SerializeField] private float minSlideTime = 2f;

    private bool activated = false;
    private float timer;


    private void Update() {
        if (!activated || !barrier.gameObject.activeInHierarchy) {
            return;
        }

        if (ClientInput.Singleton.GetPlayerActions().Slide.IsPressed()) {
            timer += Time.deltaTime;
            if (timer >= minSlideTime) {
                barrier.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player")) {
            activated = true;
        }
    }
}
