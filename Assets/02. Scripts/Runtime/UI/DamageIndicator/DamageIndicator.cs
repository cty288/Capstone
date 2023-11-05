using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.Pool;
using UnityEngine;
using UnityEngine.UI;

public class DamageIndicator : PoolableGameObject {
	private Image[] images;
	private Sequence sequence;
	private float cumulativeDamageStrength = 0;
	private Action<DamageIndicator> onFadeComplete;
	
	[SerializeField] private Vector2 fadeTimeRange = new Vector2(0.5f, 1f);
	[SerializeField] private float initialDamageStrength = 0.3f;
	
	private void Awake() {
		images = GetComponentsInChildren<Image>(true);
		sequence = DOTween.Sequence();
		cumulativeDamageStrength = initialDamageStrength;
		StartFade(0,true);
	}

	protected void StartFade(float time, bool instant) {
		if (!instant) {
			//sequence = DOTween.Sequence();
			foreach (Image image in images) {
				sequence.Append(image.DOFade(0, time));
			}
			sequence.OnComplete(OnFadeDone);
			sequence.Play();
		}
		else {
			foreach (Image image in images) {
				image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
			}
		}
	}

	private void OnFadeDone() {
		cumulativeDamageStrength = initialDamageStrength;
		sequence?.Kill();
		sequence = null;
		onFadeComplete?.Invoke(this);
		onFadeComplete = null;
		StartFade(0, true);
	}

	public void UpdateDamage(float damageStrength) {
		cumulativeDamageStrength = Mathf.Clamp01(cumulativeDamageStrength + damageStrength);
		sequence?.Kill();
		sequence = null;
		foreach (Image image in images) {
			sequence = DOTween.Sequence();
			sequence.Append(image.DOFade(cumulativeDamageStrength, 0.1f));
			//wait for a while
			sequence.AppendInterval(0.1f);
		}

		StartFade(Mathf.Lerp(fadeTimeRange.x, fadeTimeRange.y, cumulativeDamageStrength), false);
	}
	
	public void RegisterFadeCompleteCallback(Action<DamageIndicator> callback) {
		onFadeComplete += callback;
	}

	public override void OnRecycled() {
		base.OnRecycled();
		
	}

	public override void OnStartOrAllocate() {
		base.OnStartOrAllocate();
	}
}
