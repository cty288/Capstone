using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using TMPro;
using UnityEngine;

public class CurrencyIndicatorViewController : MonoBehaviour {
    private TMP_Text titleText;
    //private TMP_Text amountText;
    private float targetAmount;
    private float displayAmount;
    private Color normalColor;
    private CurrencyType currencyType;
    private void Awake() {
        titleText = transform.Find("Num").GetComponent<TMP_Text>();
        //amountText = transform.Find("Num").GetComponent<TMP_Text>();
    }

    public void Init(int amount, Color normalColor, CurrencyType currencyType) {
        //amountText.text = amount.ToString();
        displayAmount = amount;
        targetAmount = amount;
        this.normalColor = normalColor;
        titleText.color = normalColor;
        this.currencyType = currencyType;
        OnEnable();
    }
    
    public void OnAmountChanged(int amount) {
        targetAmount = amount;
    }

    private void Update() {
        if (Mathf.Abs(displayAmount - targetAmount) > 0.1f) {
            displayAmount = Mathf.Lerp(displayAmount, targetAmount, Time.deltaTime * 5f);
            titleText.text = $"<sprite index={(int) currencyType}> " + Mathf.RoundToInt(displayAmount).ToString();
            
            if (displayAmount > targetAmount) {
                titleText.color = Color.Lerp(titleText.color, Color.red, Time.deltaTime * 5f);
            }
            else {
                titleText.color = Color.Lerp(titleText.color, Color.green, Time.deltaTime * 5f);
            }
        }
        else {
            titleText.color = Color.Lerp(titleText.color, normalColor, Time.deltaTime * 5f);
        }
    }

    private void OnEnable() {
        displayAmount = targetAmount;
        titleText.text = $"<sprite index={(int) currencyType}> " + Mathf.RoundToInt(displayAmount).ToString();
        titleText.color = normalColor;
    }
}
