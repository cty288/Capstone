using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskElementViewController : MonoBehaviour {
    private TMP_Text titleText;
    private Toggle completedToggle;
    private void Awake() {
        titleText = transform.Find("Title").GetComponent<TMP_Text>();
        completedToggle = transform.Find("CompletedToggle").GetComponent<Toggle>();
    }

    public void Init(string title) {
        if (!titleText) {
            Awake();
        }
        titleText.text = title;
    }
    
    public void SetCompleted(bool completed) {
        completedToggle.isOn = completed;
    }
}
