using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using Polyglot;
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


    public virtual void SetContent(string iconNameText, string descriptionText, string iconPrefabName) {
        string colon = Localization.Get("COLON");
        if (string.IsNullOrEmpty(iconNameText)) {
            this.descriptionText.text = descriptionText;
        }
        else {
            this.descriptionText.text = $"<b>{iconNameText}{colon}</b>{descriptionText}";
        }
        

        if (!string.IsNullOrEmpty(iconPrefabName)) {
            iconSpawnPoint.gameObject.SetActive(true);
            GameObject prefab = resLoader.LoadSync<GameObject>(iconPrefabName);
            GameObject icon = GameObject.Instantiate(prefab, iconSpawnPoint);
            
            //set left right top bottom to 0
            icon.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            icon.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        }
        else {
            iconSpawnPoint.gameObject.SetActive(false);
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
