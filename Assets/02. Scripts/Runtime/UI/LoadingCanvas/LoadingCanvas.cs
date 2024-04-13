using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Singletons;
using Polyglot;
using TMPro;
using UnityEngine;

public class LoadingCanvas : MonoMikroSingleton<LoadingCanvas> {
    [SerializeField] protected GameObject loadingPanel;
    [SerializeField] protected TMP_Text loadingText;

    [SerializeField] protected float textDotInterval = 0.3f;
    
    private string[] dots = new string[] {"", ".", "..", "..."};
    private int dotIndex = 0;
    private float dotTimer = 0f;
    public bool IsLoading = false;

    private Animator animator;
    
    private void Awake() {
	    animator = loadingPanel.GetComponent<Animator>();
	    loadingText.text = Localization.Get("MENU_IO_LOAD_GAME");
    }

    private void Update() {
	    
	    dotTimer += Time.unscaledDeltaTime;
	    if (dotTimer >= textDotInterval) {
		    dotTimer = 0f;
		    dotIndex++;
		    if (dotIndex >= dots.Length) {
			    dotIndex = 0;
		    }

		    loadingText.text = Localization.Get("MENU_IO_LOAD_GAME") + dots[dotIndex];
	    }
    }

    public void Show(Action onScreenBlack) {
	    if (IsLoading) {
		    return;
	    }
	    IsLoading = true;
	    StopAllCoroutines();
	    loadingPanel.SetActive(true);
	    StartCoroutine(OnScreenBlack(onScreenBlack));
    }
    
    private IEnumerator OnScreenBlack(Action onScreenBlack) {
	    yield return new WaitForSecondsRealtime(1f);
	    IsLoading = false;
	    onScreenBlack?.Invoke();
    }
    
    public void ShowUntil(Action onScreenBlack, Func<bool> condition) {
	    if (IsLoading) {
		    return;
	    }
	    Show(onScreenBlack);
	    StartCoroutine(ShowUntilCondition(condition));
	}

    private IEnumerator ShowUntilCondition(Func<bool> condition) {
	    while (!condition()) {
		    yield return null;
	    }

	    Hide();
    }

    public void Hide() {
	    StopAllCoroutines();
	    animator.SetTrigger("Stop");
	    StartCoroutine(HideAfterSeconds(1f));
    }
    
    private IEnumerator HideAfterSeconds(float seconds) {
	    yield return new WaitForSecondsRealtime(seconds);
	    loadingPanel.SetActive(false);
	}
}
