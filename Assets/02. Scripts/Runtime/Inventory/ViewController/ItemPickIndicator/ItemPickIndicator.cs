using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Polyglot;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using TMPro;
using UnityEngine;

public class ItemStackingInfo {
	public int Count;
	public float RemainingTime;
}

public class ItemPickIndicator : AbstractMikroController<MainGame> {
	private SmoothScrollingVerticalLayoutGroup smoothScroller;
	private ICurrencyModel currencyModel;
	
	[SerializeField] private GameObject itemNameTextPrefab;
	private float hideTimer = 5f;
	[SerializeField]
	private float itemStakingTime = 0.5f;

	private Dictionary<string, ItemStackingInfo> itemStackingInfo = new Dictionary<string, ItemStackingInfo>();
	private TMP_Text itemNameHUD;
	private List<TMP_Text> itemNameTexts = new List<TMP_Text>();

	private Sequence itemHUDSequence;
	private enum FadeState {
		FadeIn,
		FadeOut,
		Idle
	}

	private FadeState fadeState = FadeState.Idle;
	private void Awake() {
		itemHUDSequence = DOTween.Sequence();
		smoothScroller = transform.Find("ScrollView").GetComponent<SmoothScrollingVerticalLayoutGroup>();
		itemNameHUD = transform.Find("ItemNameHUD").GetComponent<TMP_Text>();
		currencyModel = this.GetModel<ICurrencyModel>();
		this.RegisterEvent<OnInventoryItemAddedEvent>(OnInventoryItemAdded)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
		this.RegisterEvent<OnCurrentHotbarUpdateEvent>(OnCurrentHotbarUpdate)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
		
		this.RegisterEvent<OnCurrencyAmountChangedEvent>(OnCurrencyAmountChanged)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
		
		this.RegisterEvent<OnMoneyAmountChangedEvent>(OnMoneyAmountChanged)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
	}

	private void OnMoneyAmountChanged(OnMoneyAmountChangedEvent e) {
		string currencyTypeLocalized = $"<sprite index=6>";
		StackItem(currencyTypeLocalized, e.Amount);
	}

	private void OnCurrencyAmountChanged(OnCurrencyAmountChangedEvent e) {
		if (!e.IsTransferToMoney) {
			string currencyTypeLocalized = $"<sprite index={(int) e.CurrencyType}>";
			StackItem(currencyTypeLocalized, e.Amount);
		}
		else {
			//Show();
			Show(Localization.GetFormat("HINT_CURRENCY_TRANSFER",
				$"{e.Amount} <sprite index={(int) e.CurrencyType}>",
				$"{e.TransferAmount} <sprite index=6>"));
		}
		
	}

	private void OnCurrentHotbarUpdate(OnCurrentHotbarUpdateEvent e) {
		IResourceEntity item = e.TopItem;
		itemHUDSequence.Kill();
		if (item == null) {
			/*itemNameHUD.DOFade(0f, 0.5f).OnComplete(() => {
				itemNameHUD.text = "";
			});*/
			itemHUDSequence = DOTween.Sequence();
			itemHUDSequence.Append(itemNameHUD.DOFade(0f, 0.5f));
			itemHUDSequence.AppendCallback(() => {
				itemNameHUD.text = "";
			});
		}else {
			itemHUDSequence = DOTween.Sequence();
			itemNameHUD.text = item.GetDisplayName();
			
			itemHUDSequence.Append(itemNameHUD.DOFade(1f, 0.5f));
			itemHUDSequence.AppendInterval(3f);
			itemHUDSequence.Append(itemNameHUD.DOFade(0f, 0.5f));
			
		}

		itemHUDSequence.Play();
	}

	private void OnInventoryItemAdded(OnInventoryItemAddedEvent e) {
		IResourceEntity item = e.Item;
		if(item == null) return;
		string displayName = item.GetDisplayName();

		StackItem(displayName, 1);
	}
	
	private void StackItem(string displayName, int count) {
		if (itemStackingInfo.TryGetValue(displayName, out var info)) {
			info.Count += count;
		}else {
			itemStackingInfo.Add(displayName, new ItemStackingInfo() {
				Count = count,
				RemainingTime = itemStakingTime
			});
		}
	}
	

	private void ShowStackedItem(string itemDisplayName) {
		if (!itemStackingInfo.ContainsKey(itemDisplayName)) {
			return;
		}
		
		int count = itemStackingInfo[itemDisplayName].Count;
		string displayName = itemDisplayName;
		
		string content = "";
		if (count > 0) {
			content = $"<color=green>+{count}</color> " + displayName;
		}
		else {
			content = $"<color=red>{count}</color> " + displayName;
		}
		Show(content);
		//itemStackingInfo.Remove(itemDisplayName);
	}

	private void Show(string content) {
		if (hideTimer <= 0f && fadeState == FadeState.Idle) {
			smoothScroller.gameObject.SetActive(true);
			smoothScroller.Clear();
		}else if (fadeState == FadeState.FadeOut) {
			smoothScroller.gameObject.SetActive(true);
			Fade(true, null);
		}
		
		GameObject itemNameText = Instantiate(itemNameTextPrefab);
		TMP_Text text = itemNameText.GetComponent<TMP_Text>();
		smoothScroller.AddNewItem(itemNameText);
		itemNameTexts.Add(text);
		hideTimer = 5f;
		text.text = content;
	}

	private void Update() {
		HashSet<string> keysToRemove = new HashSet<string>();
		foreach (string displayName in itemStackingInfo.Keys) {
			ItemStackingInfo info = itemStackingInfo[displayName];
			info.RemainingTime -= Time.deltaTime;
			if (info.RemainingTime <= 0f) {
				ShowStackedItem(displayName);
				keysToRemove.Add(displayName);
			}
		}
		
		foreach (string key in keysToRemove) {
			itemStackingInfo.Remove(key);
		}
		
		
		float lastHideTimer = hideTimer;
		hideTimer -= Time.deltaTime;
		if (lastHideTimer > 0f && hideTimer <= 0f) {
			Fade(false, () => {
				smoothScroller.Clear();
				smoothScroller.gameObject.SetActive(false);
			});
		}
	}
	
	private void Fade(bool fadeIn, Action onComplete) {
		fadeState = fadeIn ? FadeState.FadeIn : FadeState.FadeOut;
		bool completed = false;
		foreach (var text in itemNameTexts) {
			text.DOKill();
			text.DOFade(fadeIn ? 1f : 0f, 0.5f).OnComplete(() => {
				fadeState = FadeState.Idle;
				if (!completed) {
					completed = true;
					onComplete?.Invoke();
				}
			});
		}
	}
}
