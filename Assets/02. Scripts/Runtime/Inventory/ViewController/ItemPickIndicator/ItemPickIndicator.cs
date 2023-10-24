using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
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
		this.RegisterEvent<OnInventoryItemAddedEvent>(OnInventoryItemAdded)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
		this.RegisterEvent<OnCurrentHotbarUpdateEvent>(OnCurrentHotbarUpdate)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
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

		if (itemStackingInfo.TryGetValue(displayName, out var info)) {
			info.Count++;
		}else {
			itemStackingInfo.Add(displayName, new ItemStackingInfo() {
				Count = 1,
				RemainingTime = itemStakingTime
			});
		}
	}
	

	private void ShowItem(string itemDisplayName) {
		if (!itemStackingInfo.ContainsKey(itemDisplayName)) {
			return;
		}
		
		if (hideTimer <= 0f && fadeState == FadeState.Idle) {
			smoothScroller.gameObject.SetActive(true);
			smoothScroller.Clear();
		}else if (fadeState == FadeState.FadeOut) {
			smoothScroller.gameObject.SetActive(true);
			Fade(true, null);
		}
		
		int count = itemStackingInfo[itemDisplayName].Count;
		string displayName = itemDisplayName;
		
		
		GameObject itemNameText = Instantiate(itemNameTextPrefab);
		TMP_Text text = itemNameText.GetComponent<TMP_Text>();
		text.text = $"<color=green>+{count}</color> " + displayName;
		smoothScroller.AddNewItem(itemNameText);
	
		itemNameTexts.Add(text);
		hideTimer = 5f;

		//itemStackingInfo.Remove(itemDisplayName);
	}

	private void Update() {
		HashSet<string> keysToRemove = new HashSet<string>();
		foreach (string displayName in itemStackingInfo.Keys) {
			ItemStackingInfo info = itemStackingInfo[displayName];
			info.RemainingTime -= Time.deltaTime;
			if (info.RemainingTime <= 0f) {
				ShowItem(displayName);
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
