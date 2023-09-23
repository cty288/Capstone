using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotResourceDescriptionPanel : PoolableGameObject, IController {
    private TMP_Text nameText;
    private TMP_Text descriptionText;
    private RectTransform rectTransform;
    private bool isShowing = false;

    public bool IsShowing => isShowing;
    public void Awake() {
        nameText = transform.Find("NameText").GetComponent<TMP_Text>();
        descriptionText = transform.Find("DescriptionText").GetComponent<TMP_Text>();
        rectTransform = GetComponent<RectTransform>();
    }
    
    public void SetContent(string name, string description) {
        
        nameText.text = name;
        descriptionText.text = description;
        if (gameObject.activeInHierarchy) {
            StartCoroutine(RebuildLayout());
        }
    }

    public void Show() {
        gameObject.SetActive(true);
        isShowing = true;
    }
    public void Hide() {
        gameObject.SetActive(false);
        isShowing = false;
    }



    private IEnumerator RebuildLayout() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public override void OnRecycled() {
        base.OnRecycled();
        SetContent("", "");
    }

    public IArchitecture GetArchitecture() {
        return MainGame.Interface;
    }
}
