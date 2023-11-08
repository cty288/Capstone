using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Currency.Model;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using Polyglot;
using UnityEngine;

public class CurrencyIndicatorUIPanel : AbstractMikroController<MainGame> {
	private Dictionary<CurrencyType, CurrencyIndicatorViewController> currencyIndicatorViewControllers =
		new Dictionary<CurrencyType, CurrencyIndicatorViewController>();
	private Transform spawnTransform;
	
	[SerializeField] private GameObject currencyIndicatorPrefab;
	private ICurrencyModel currencyModel;
	[SerializeField] private Color textColor = Color.white;
	private void Awake() {
		currencyModel = this.GetModel<ICurrencyModel>();
		spawnTransform = transform.Find("Panel");
		
		foreach (CurrencyType currencyType in Enum.GetValues(typeof(CurrencyType))) {
			GameObject currencyIndicator = Instantiate(currencyIndicatorPrefab, spawnTransform);
			CurrencyIndicatorViewController currencyIndicatorViewController =
				currencyIndicator.GetComponent<CurrencyIndicatorViewController>();

			BindableProperty<int> currencyAmount = currencyModel.GetCurrencyAmountProperty(currencyType);

			currencyIndicatorViewController.Init(Localization.Get($"CURRENCY_{currencyType.ToString()}_name") + ":",
				currencyAmount.Value, textColor);
			currencyIndicatorViewControllers.Add(currencyType, currencyIndicatorViewController);
			

		}

		this.RegisterEvent<OnCurrencyAmountChangedEvent>(OnCurrencyAmountChanged)
			.UnRegisterWhenGameObjectDestroyed(gameObject);

	}

	private void OnCurrencyAmountChanged(OnCurrencyAmountChangedEvent e) {
		currencyIndicatorViewControllers[e.CurrencyType].OnAmountChanged(e.CurrentAmount);
	}

	private void Start() {
		
	}

	
}
