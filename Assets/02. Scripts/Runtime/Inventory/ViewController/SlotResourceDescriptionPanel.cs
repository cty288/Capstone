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
    private Image icon;
    
    [SerializeField]
    private Vector2 leftPivot = new Vector2(-3.72529e-08f, 1f);
    
    [SerializeField]
    private Vector2 rightPivot = new Vector2(1f, 1f);

    public bool IsShowing => isShowing;
    public void Awake() {
        nameText = transform.Find("NameText").GetComponent<TMP_Text>();
        descriptionText = transform.Find("DescriptionText").GetComponent<TMP_Text>();
        rectTransform = GetComponent<RectTransform>();
        icon = transform.Find("Icon").GetComponent<Image>();
    }
    
    public void SetContent(string name, string description, Sprite sprite, bool isLeftPivot) {
        
        nameText.text = name;
        descriptionText.text = description;
        if (gameObject.activeInHierarchy) {
            StartCoroutine(RebuildLayout());
        }
        if (sprite != null) {
            icon.gameObject.SetActive(true);
            icon.sprite = sprite;
        }
        else {
            icon.gameObject.SetActive(false);
        }
        
        if (isLeftPivot) {
            rectTransform.pivot = leftPivot;
        }
        else {
            rectTransform.pivot = rightPivot;
        }
    }

    public void Show() {
        gameObject.SetActive(true);
        isShowing = true;
        StartCoroutine(RebuildLayout());
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
        SetContent("", "", null, true);
    }

    public IArchitecture GetArchitecture() {
        return MainGame.Interface;
    }
}
