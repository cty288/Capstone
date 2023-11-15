using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using TMPro;
using UnityEngine;

public class MoneyIndicator : AbstractMikroController<MainGame> {
	private ILevelModel levelModel;
	private TMP_Text currencyText;
	private ICurrencyModel currencyModel;
	
	private float targetAmount;
	private float displayAmount;

	[SerializeField] private Color normalColor = Color.white;
	private void Awake() {
		levelModel = this.GetModel<ILevelModel>();
		currencyText = transform.Find("CurrencyText").GetComponent<TMP_Text>();
		levelModel.CurrentLevelCount.RegisterWithInitValue(OnCurrentLevelCountChanged)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
		currencyModel = this.GetModel<ICurrencyModel>();
		currencyModel.Money.RegisterWithInitValue(OnMoneyChanged)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
		
	}

	private void OnMoneyChanged(int old, int newMoney) {
		targetAmount = newMoney;
	}
	
	private void Update() {
		if (Mathf.Abs(displayAmount - targetAmount) > 0.1f) {
			displayAmount = Mathf.Lerp(displayAmount, targetAmount, Time.unscaledDeltaTime * 5f);
			currencyText.text = $"<sprite index=6> " + Mathf.RoundToInt(displayAmount).ToString();
            
			if (displayAmount > targetAmount) {
				currencyText.color = Color.Lerp(currencyText.color, Color.red, Time.unscaledDeltaTime * 5f);
			}
			else {
				currencyText.color = Color.Lerp(currencyText.color, Color.green, Time.unscaledDeltaTime * 5f);
			}
		}
		else {
			currencyText.color = Color.Lerp(currencyText.color, normalColor, Time.unscaledDeltaTime * 5f);
		}
	}


	private void OnCurrentLevelCountChanged(int level) {
		currencyText.gameObject.SetActive(level == 0);
	}
	
	private void OnEnable() {
		displayAmount = targetAmount;
		currencyText.text =  $"<sprite index=6> " + Mathf.RoundToInt(displayAmount).ToString();
		currencyText.color = normalColor;
	}
}
