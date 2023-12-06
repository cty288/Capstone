using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyIndicatorViewController : MonoBehaviour {
    private TMP_Text titleText;
    private Image bgImage;
    //private TMP_Text amountText;
    private float targetAmount;
    private float displayAmount;
    private Color normalColor;
    private CurrencyType currencyType;

    [SerializedDictionary("CurrencyType", "Sprite")] [SerializeField]
    private SerializedDictionary<CurrencyType, Sprite> currencySprites = new SerializedDictionary<CurrencyType, Sprite>();
    
    private void Awake() {
        titleText = transform.Find("Num").GetComponent<TMP_Text>();
        bgImage = transform.Find("BG").GetComponent<Image>();
        //amountText = transform.Find("Num").GetComponent<TMP_Text>();
    }

    public void Init(int amount, Color normalColor, CurrencyType currencyType) {
        //amountText.text = amount.ToString();
        displayAmount = amount;
        targetAmount = amount;
        this.normalColor = normalColor;
        titleText.color = normalColor;
        this.currencyType = currencyType;
        bgImage.sprite = currencySprites[currencyType];
        OnEnable();
    }
    
    public void OnAmountChanged(int amount) {
        targetAmount = amount;
    }

    private void Update() {
        if (Mathf.Abs(displayAmount - targetAmount) > 0.1f) {
            displayAmount = Mathf.Lerp(displayAmount, targetAmount, Time.unscaledDeltaTime * 5f);
            titleText.text = Mathf.RoundToInt(displayAmount).ToString();
            
            if (displayAmount > targetAmount) {
                titleText.color = Color.Lerp(titleText.color, Color.red, Time.unscaledDeltaTime * 5f);
            }
            else {
                titleText.color = Color.Lerp(titleText.color, Color.green, Time.unscaledDeltaTime * 5f);
            }
        }
        else {
            titleText.color = Color.Lerp(titleText.color, normalColor, Time.unscaledDeltaTime * 5f);
        }
    }

    private void OnEnable() {
        displayAmount = targetAmount;
        titleText.text = Mathf.RoundToInt(displayAmount).ToString();
        titleText.color = normalColor;
        bgImage.sprite = currencySprites[currencyType];
    }
}
