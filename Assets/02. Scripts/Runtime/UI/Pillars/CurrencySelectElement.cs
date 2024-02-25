using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using Framework;
using MikroFramework.Architecture;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class CurrencySelectElement : AbstractMikroController<MainGame> {
    [SerializeField] private CurrencyType currencyType;

    public CurrencyType CurrencyType => currencyType;
    
    [SerializeField] private Sprite selectedIconSprite;
    [SerializeField] private Sprite unselectedIconSprite;
    [SerializeField] private bool overrideSpriteColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unselectedColor;
    
    
    private Image icon;
    private TMP_Text nameText;
    private Toggle toggle;

    private void Awake() {
        toggle = GetComponent<Toggle>();
        icon = transform.Find("Icon").GetComponent<Image>();
        nameText = transform.Find("NameText").GetComponent<TMP_Text>();
        
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
        OnToggleValueChanged(toggle.isOn);
    }

    private void OnToggleValueChanged(bool isOn) {
        icon.sprite = isOn ? selectedIconSprite : unselectedIconSprite;
        nameText.color = isOn ? Color.white : Color.black;
        if (overrideSpriteColor) {
            icon.color = isOn ? selectedColor : unselectedColor;
        }
    }
}
