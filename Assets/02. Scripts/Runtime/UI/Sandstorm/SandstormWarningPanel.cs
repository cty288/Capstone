using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Sandstorm;
using DG.Tweening;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using Runtime.Utilities;
using TMPro;
using UnityEngine;

public class SandstormWarningPanel : AbstractMikroController<MainGame> {
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject approachingPanel;

    [SerializeField] private TMP_Text remainingTimeText;

    private ILevelModel levelModel;
    private TimeSpan remainingSandstormTime = TimeSpan.Zero;
    private IGameTimeModel gameTimeModel;
    private bool timeNotEnough = false;
    private bool animationTriggered = false;
    private Color originalColor;
    private Coroutine blinkCoroutine = null;
    private void Awake() {
        levelModel = this.GetModel<ILevelModel>();
        levelModel.CurrentLevelCount.RegisterWithInitValue(OnLevelCountChanged)
            .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        gameTimeModel = this.GetModel<IGameTimeModel>();
        originalColor = remainingTimeText.color;
        
        this.RegisterEvent<OnSandStormWarning>(OnSandStormWarning);
        gameTimeModel.GlobalTime.RegisterWithInitValue(OnGlobalTimeChanged)
            .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
    }

    private void OnGlobalTimeChanged(DateTime oldTime, DateTime newTime) {
        int minutes = (int) (newTime - oldTime).TotalMinutes;
        timeNotEnough = false;
        
        if (remainingSandstormTime.TotalMinutes > 0) {
            remainingSandstormTime = remainingSandstormTime.Subtract(TimeSpan.FromMinutes(minutes));
            remainingTimeText.text = $"{remainingSandstormTime.Hours}:{remainingSandstormTime.Minutes}";
            
            if (remainingSandstormTime.TotalMinutes <= 60) {
                timeNotEnough = true;
                if (!animationTriggered) {
                    animationTriggered = true;
                    blinkCoroutine = StartCoroutine(BlinkText());
                }
            }
        }
        else {
            Hide();
        }
    }
    
    private IEnumerator BlinkText() {
        while (true) {
            remainingTimeText.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            remainingTimeText.color = originalColor;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void Update() {
        
    }

    private void OnSandStormWarning(OnSandStormWarning e) {
        mainPanel.gameObject.SetActive(true);
        approachingPanel.gameObject.SetActive(true);
        this.Delay(4f, () => {
            approachingPanel.gameObject.SetActive(false);
        });
        remainingSandstormTime = TimeSpan.FromMinutes(e.RemainingMinutes);
    }

    private void OnLevelCountChanged(int arg1, int day) {
        Hide();
    }
    
    private void Hide() {
        mainPanel.gameObject.SetActive(false);
        approachingPanel.gameObject.SetActive(false);
        if (animationTriggered) {
            if (blinkCoroutine != null) {
                StopCoroutine(blinkCoroutine);
            }
            remainingTimeText.color = originalColor;
            blinkCoroutine = null;
        }
        animationTriggered = false;
        remainingSandstormTime = TimeSpan.Zero;
    }
}
