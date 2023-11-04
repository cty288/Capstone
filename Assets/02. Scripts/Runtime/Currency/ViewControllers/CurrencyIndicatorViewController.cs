using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyIndicatorViewController : MonoBehaviour {
    private TMP_Text titleText;
    private TMP_Text amountText;
    private float targetAmount;
    private float displayAmount;

    private void Awake() {
        titleText = transform.Find("Title").GetComponent<TMP_Text>();
        amountText = transform.Find("Num").GetComponent<TMP_Text>();
    }

    public void Init(string title, int amount) {
        amountText.text = amount.ToString();
        titleText.text = title;
        displayAmount = amount;
        targetAmount = amount;
    }
    
    public void OnAmountChanged(int amount) {
        targetAmount = amount;
    }

    private void Update() {
        if (Mathf.Abs(displayAmount - targetAmount) > 0.1f) {
            displayAmount = Mathf.Lerp(displayAmount, targetAmount, Time.deltaTime * 5f);
            amountText.text = Mathf.RoundToInt(displayAmount).ToString();
            
            if (displayAmount > targetAmount) {
                amountText.color = Color.Lerp(amountText.color, Color.red, Time.deltaTime * 5f);
            }
            else {
                amountText.color = Color.Lerp(amountText.color, Color.green, Time.deltaTime * 5f);
            }
        }
        else {
            amountText.color = Color.Lerp(amountText.color, Color.white, Time.deltaTime * 5f);
        }
    }
}
