using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PressHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    private Button button;

    [SerializeField] private float pressHoldThreshold = 0.5f;
    [SerializeField] private float holdCallbackIntervalDecrementPerSecond = 0.1f;
    [SerializeField] private float minHoldCallbackInterval = 0.1f;
    //the longer you hold, the faster the callback will be called (shorter interval)
    [SerializeField] private float initialHoldCallbackInterval = 0.5f;
    private float currentHoldCallbackInterval;
    private float holdTimer = 0f;
    private float holdCallbackTimer = 0f;
    private bool isHolding = false;

    
    private Action onCallback;
    
    private void Awake() {
        button = GetComponent<Button>();
        currentHoldCallbackInterval = initialHoldCallbackInterval;
    }

    public void OnPointerDown(PointerEventData eventData) {
        currentHoldCallbackInterval = initialHoldCallbackInterval;
        holdTimer = 0f;
        isHolding = true;
    }

    private void Update() {
        if (isHolding) {
            holdTimer += Time.unscaledDeltaTime;
            if (holdTimer >= pressHoldThreshold) {
                if (currentHoldCallbackInterval > minHoldCallbackInterval) {
                    currentHoldCallbackInterval -= holdCallbackIntervalDecrementPerSecond * Time.unscaledDeltaTime;
                }
                holdCallbackTimer += Time.unscaledDeltaTime;
                if (holdCallbackTimer >= currentHoldCallbackInterval) {
                    holdCallbackTimer = 0f;
                    onCallback?.Invoke();
                }
            }
        }
    }

    public void RegisterCallback(Action callback) {
        onCallback = callback;
    }
    
    public void OnPointerUp(PointerEventData eventData) {
        if (isHolding) {
            isHolding = false;
            if (holdTimer < pressHoldThreshold) {
                onCallback?.Invoke();
            }
            holdTimer = 0f;
            holdCallbackTimer = 0f;
            currentHoldCallbackInterval = initialHoldCallbackInterval;
        }
    }
}
