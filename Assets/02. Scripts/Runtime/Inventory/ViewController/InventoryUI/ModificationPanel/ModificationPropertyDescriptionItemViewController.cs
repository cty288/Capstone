using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Polyglot;
using TMPro;
using UnityEngine;

public class ModificationPropertyDescriptionItemViewController : AbstractMikroController<MainGame> {
    private ResLoader resLoader;
    private TMP_Text propertyNameText;
    private TMP_Text propertyValueText;
    private Transform iconSpawnPoint;

    private void Awake() {
        resLoader = this.GetUtility<ResLoader>();
        propertyNameText = transform.Find("TextGroup/PropertyName").GetComponent<TMP_Text>();
        propertyValueText = transform.Find("TextGroup/PropertyValue").GetComponent<TMP_Text>();
        iconSpawnPoint = transform.Find("Icon");
    }
    
    public void SetContent(string iconNameText, string descriptionText, string iconPrefabName) {
        string colon = Localization.Get("COLON");
        propertyNameText.gameObject.SetActive(!string.IsNullOrEmpty(iconNameText));
        propertyValueText.gameObject.SetActive(!string.IsNullOrEmpty(descriptionText));
        
        if (!string.IsNullOrEmpty(iconNameText)) {
            propertyNameText.text = iconNameText;
        }
        
        if (!string.IsNullOrEmpty(descriptionText)) {
            propertyValueText.text = descriptionText;
        }


        if (!string.IsNullOrEmpty(iconPrefabName)) {
            GameObject prefab = resLoader.LoadSync<GameObject>(iconPrefabName);
            GameObject icon = GameObject.Instantiate(prefab, iconSpawnPoint);
            
            //set left right top bottom to 0
            icon.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            icon.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        }

    }
}
