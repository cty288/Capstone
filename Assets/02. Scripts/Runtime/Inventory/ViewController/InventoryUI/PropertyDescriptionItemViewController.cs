using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using TMPro;
using UnityEngine;

public class PropertyDescriptionItemViewController : PoolableGameObject, IController {
    private TMP_Text descriptionText;
    private Transform iconSpawnPoint;
    private ResLoader resLoader;
    private void Awake() {
        descriptionText = transform.Find("DescriptionText").GetComponent<TMP_Text>();
        iconSpawnPoint = transform.Find("PropertyIconSpawnPos");
        resLoader = this.GetUtility<ResLoader>();
    }


    public void SetContent(string descriptionText, string iconPrefabName) {
        this.descriptionText.text = descriptionText;

        if (!string.IsNullOrEmpty(iconPrefabName)) {
            GameObject prefab = resLoader.LoadSync<GameObject>(iconPrefabName);
            GameObject icon = GameObject.Instantiate(prefab, iconSpawnPoint);
            
            //set left right top bottom to 0
            icon.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            icon.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        }

    }

    public override void OnRecycled() {
        base.OnRecycled();
        descriptionText.text = "";
        
        foreach (Transform child in iconSpawnPoint) {
            GameObject.Destroy(child.gameObject);
        }
    }

    public IArchitecture GetArchitecture() {
        return MainGame.Interface;
    }
}
