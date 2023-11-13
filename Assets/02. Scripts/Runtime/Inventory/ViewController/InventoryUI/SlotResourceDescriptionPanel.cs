using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.GameResources.Model.Base;
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

    [SerializeField] private GameObject rarityIndicator;
    
    private Transform rarityIndicatorTransform;
    private List<GameObject> spawnedPropertyDescriptions = new List<GameObject>();
    private Transform itemPropertyDescriptionPanel;
    private SafeGameObjectPool propertyDescriptionItemPool;
    private TMP_Text resourceDisplayedTypeText;
    [SerializeField] private GameObject propertyDescriptionItemPrefab;

    public bool IsShowing => isShowing;
    public void Awake() {
        nameText = transform.Find("NameText").GetComponent<TMP_Text>();
        descriptionText = transform.Find("DescriptionText").GetComponent<TMP_Text>();
        rectTransform = GetComponent<RectTransform>();
        icon = transform.Find("Icon").GetComponent<Image>();
        rarityIndicatorTransform = transform.Find("RarityBar");
        itemPropertyDescriptionPanel = transform.Find("ItemPropertyDescription");
        resourceDisplayedTypeText = transform.Find("ResourceDisplayedTypeText").GetComponent<TMP_Text>();
        propertyDescriptionItemPool = GameObjectPoolManager.Singleton.CreatePool(propertyDescriptionItemPrefab, 5, 8);
        
    }
    
    public void SetContent(string name, string description, Sprite sprite, bool isLeftPivot, int rarity,
    string resourceDisplayedType, List<ResourcePropertyDescription> propertyDescriptions) {
        
        nameText.text = name;
        descriptionText.text = description;
        resourceDisplayedTypeText.text = resourceDisplayedType;

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

        //int rarityNumToSpawn = rarity - rarityIndicatorTransform.childCount;
        for (int i = 0; i < rarity; i++) {
            Instantiate(rarityIndicator, rarityIndicatorTransform);
        }

        if (propertyDescriptions is {Count: > 0}) {
            itemPropertyDescriptionPanel.gameObject.SetActive(true);
            SetPropertyDescriptions(propertyDescriptions);
        }
        else {
            itemPropertyDescriptionPanel.gameObject.SetActive(false);
        }
        
        if (gameObject.activeInHierarchy) {
            StartCoroutine(RebuildLayout());
        }
        
    }

    
    private void SetPropertyDescriptions(List<ResourcePropertyDescription> propertyDescriptions) {
        spawnedPropertyDescriptions.Clear();
        foreach (Transform child in itemPropertyDescriptionPanel.transform) {
            propertyDescriptionItemPool.Recycle(child.gameObject);
        }
        
        
        foreach (ResourcePropertyDescription propertyDescription in propertyDescriptions) {
            GameObject propertyDescriptionItem = propertyDescriptionItemPool.Allocate();
            propertyDescriptionItem.transform.SetParent(itemPropertyDescriptionPanel);
            propertyDescriptionItem.transform.localScale = Vector3.one;
            propertyDescriptionItem.GetComponent<PropertyDescriptionItemViewController>()
                .SetContent(propertyDescription.localizedDescription, propertyDescription.iconName);
            
            spawnedPropertyDescriptions.Add(propertyDescriptionItem);
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
        for (int i = 0; i < rarityIndicatorTransform.childCount; i++) {
            GameObject child = rarityIndicatorTransform.GetChild(i).gameObject;
            Destroy(child);
        }
        
        foreach (var propertyDescription in spawnedPropertyDescriptions) {
            GameObjectPoolManager.Singleton.Recycle(propertyDescription.gameObject);
        }
        spawnedPropertyDescriptions.Clear();
        
        itemPropertyDescriptionPanel.gameObject.SetActive(false);
    }



    private IEnumerator RebuildLayout() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public void Clear() {
        SetContent("", "", null, true, 0, "",null);
    }
    public override void OnRecycled() {
        base.OnRecycled();
        
        Clear();
    }
    
    

    public IArchitecture GetArchitecture() {
        return MainGame.Interface;
    }
}
