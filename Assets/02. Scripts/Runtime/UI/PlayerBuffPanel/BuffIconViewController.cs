using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BuffIconViewController : MonoBehaviour {
	private IBuff buff;
	private Image progressBar;
	private Image[] buffImages;
	private bool isBlinking = false;

	private void Awake() {
		progressBar = transform.Find("BuffIconProgress").GetComponent<Image>();
		buffImages = gameObject.GetComponentsInChildren<Image>();
	}

	public void SetBuff(IBuff buff) {
		this.buff = buff;
		UpdateProgress();
	}

	public virtual void OnRefresh() {
		UpdateProgress();
	}

	private void Update() {
		UpdateProgress();
		if (buff.MaxDuration > 0) {
			//if remaining duration is less than 5 seconds, blink the buff icon
			if (buff.RemainingDuration <= 5f) {
				if (!isBlinking) {
					isBlinking = true;
					foreach (Image buffImage in buffImages) {
						buffImage.DOKill();
						buffImage.DOFade(0f, 0.5f).SetLoops(-1, LoopType.Yoyo);
					}
				}
			}
			else {
				if (isBlinking) {
					isBlinking = false;
					foreach (Image buffImage in buffImages) {
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
