using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using MikroFramework.AudioKit;
using MikroFramework.BindableProperty;
using Polyglot;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencySelectSlider : MonoBehaviour
{
    private PressHoldButton addCurrencyButton;
    private PressHoldButton minusCurrencyButton;
    private TMP_Text currencySliderText;
    private Slider currencySlider;

    private int maxCurrencyPossible = 0;
    
    [SerializeField] private Color color;
    private Image sliderFillImage;
    public BindableProperty<int> currentlySelectedCurrency = new BindableProperty<int>(0);
    private void Awake() {
        currencySlider = GetComponent<Slider>();
        addCurrencyButton = transform.Find("AddCurrencyButton").GetComponent<PressHoldButton>();
        minusCurrencyButton = transform.Find("RemoveCurrencyButton").GetComponent<PressHoldButton>();
        currencySliderText = transform.Find("CostText").GetComponent<TMP_Text>();
        sliderFillImage = currencySlider.fillRect.GetComponent<Image>();
        sliderFillImage.color = color;
        addCurrencyButton.RegisterCallback(OnAddCurrencyButtonClicked);
        minusCurrencyButton.RegisterCallback(OnMinusCurrencyButtonClicked);
    }
    
    public void ResetInfo(int maxCurrency) {
        maxCurrencyPossible = maxCurrency;
       
        SelectCurrency(0);
    }
    
    public void SetAddButtonActive(bool active) {
        addCurrencyButton.gameObject.SetActive(active);
    }

    private void OnMinusCurrencyButtonClicked() {
        SelectCurrency(currentlySelectedCurrency.Value - 1);
    }

    private void OnAddCurrencyButtonClicked() {
        SelectCurrency(currentlySelectedCurrency.Value + 1);
    }
    
    public void SelectCurrency(int currency) {
        if (currency < 0) {
            currency = 0;
        }else if (currency > maxCurrencyPossible) {
            currency = maxCurrencyPossible;
        }
       

        AudioSystem.Singleton.Play2DSound("arrow_click");
        currentlySelectedCurrency.Value = currency;
        currencySliderText.text = currency.ToString();

        float ratio = maxCurrencyPossible == 0 ? 0 : (float) currency / maxCurrencyPossible;
        currencySlider.value = ratio;
    }
}
