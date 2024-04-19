using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffIconViewController : MonoBehaviour {
	private IBuff buff;
	[SerializeField]
	private Image progressBar;

	[SerializeField] private TMP_Text buffLevelText;
	[SerializeField] private TMP_Text buffNameText;
	
	private MaskableGraphic[] buffImages;
	private bool isBlinking = false;

	private void Awake() {
		progressBar = transform.Find("BuffIconProgress").GetComponent<Image>();
		buffImages = gameObject.GetComponentsInChildren<MaskableGraphic>(true);
	}

	public void SetBuff(IBuff buff) {
		this.buff = buff;
		this.buffNameText.text = buff.GetDisplayName();
		buffLevelText.gameObject.SetActive(false);
		if (this.buff is ILeveledBuff leveledBuff) {
			buffLevelText.gameObject.SetActive(true);
			buffLevelText.text = leveledBuff.Level.ToString();
		}
		UpdateProgress();
	}

	public virtual void OnRefresh() {
		UpdateProgress();
	}

	private void Update() {
		UpdateProgress();
		if (buff.MaxDuration > 0) {
			float progress = buff.RemainingDuration / buff.MaxDuration;
			//if remaining duration is less than 5 seconds, blink the buff icon
			if (progress <= 0.2f && buff.RemainingDuration <= 5f) {
				if (!isBlinking) {
					isBlinking = true;
					foreach (MaskableGraphic buffImage in buffImages) {
						buffImage.DOKill();
						buffImage.DOFade(0f, 0.5f).SetLoops(-1, LoopType.Yoyo);
					}
				}
			}
			else {
				if (isBlinking) {
					isBlinking = false;
					foreach (MaskableGraphic buffImage in buffImages) {
						buffImage.DOKill();
						buffImage.DOFade(1f, 0.5f);
					}
				}
			}
			
		}
	}

	private void UpdateProgress() {
		if(buff == null) return;
		if (buff.MaxDuration <= 0) {
			progressBar.fillAmount = 1;
		}
		else {
			progressBar.fillAmount = buff.RemainingDuration / buff.MaxDuration;
		}
	}

}
