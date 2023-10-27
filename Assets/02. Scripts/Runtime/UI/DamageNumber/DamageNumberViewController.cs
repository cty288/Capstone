using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.ResKit;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DamageNumberViewController : DefaultPoolableGameObject {
	private TMP_Text text;

	[SerializeField] private float duration = 0.3f;
	[SerializeField] private Vector2 screenYPosRange = new Vector2(100, 200);
	[SerializeField] private Vector2 screenXPosRange = new Vector2(-50, 50);
	
	private RectTransform rectTransform;
	public Action<DamageNumberViewController> OnRecycledAction;
	private void Awake() {
		text = GetComponentInChildren<TMP_Text>();
		rectTransform = text.GetComponent<RectTransform>();
		
	}
	
	public void StartAnimateDamage(float damage, int minSizeDamage, float maxSizeDamage, float minSize, float maxSize) {
		Color targetColor = new Color(0.6226415f, 0.6226415f, 0.6226415f,1);
		

		damage = Mathf.Max(damage, 0);
		float damageNormalized = Mathf.Clamp((damage - minSizeDamage) / (maxSizeDamage - minSizeDamage), 0, 1);
		
		
		targetColor = new Color(1, 1f - damageNormalized, 1f- damageNormalized,1);

		float targetSize = Mathf.Lerp(minSize, maxSize, damageNormalized);

		//get posX and posY
		Vector3 pos = rectTransform.anchoredPosition;
		
		
		Vector2 targetPos = pos + new Vector3(Random.Range(screenXPosRange.x, screenXPosRange.y),
			Random.Range(screenYPosRange.x, screenYPosRange.y), 0);
		
		
		float targetTime = duration * (Mathf.Clamp((damage / maxSizeDamage),0,1f) + 1);
		text.text = damage.ToString();
		text.color = targetColor;
		text.DOFade(0, targetTime).OnComplete(RecycleToCache);

		rectTransform.localScale = new Vector3(targetSize, targetSize, targetSize);
		rectTransform.DOScale(new Vector3(0.3f, 0.3f, 0.3f), targetTime);

		//transform.DOLocalMove(targetPos, targetTime);
		rectTransform.DOAnchorPos(targetPos, targetTime);
	}
	
	
	public void FadeOut() {
		DOTween.Kill(rectTransform);
		text.DOFade(0, duration).OnComplete(RecycleToCache);
	}

	public override void OnRecycled() {
		base.OnRecycled();
		text.color = Color.white;
		DOTween.Kill(rectTransform);
		rectTransform.localScale = Vector3.one;
		rectTransform.anchoredPosition = Vector3.zero;
		OnRecycledAction?.Invoke(this);
		OnRecycledAction = null;
	}
}
