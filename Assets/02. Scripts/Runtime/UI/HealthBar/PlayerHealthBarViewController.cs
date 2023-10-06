using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Runtime.Enemies.Model.Properties;
using Runtime.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarViewController : AbstractMikroController<MainGame> {
    private Slider slider;
    private IGamePlayerModel playerModel;
    
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color hurtColor = Color.red;
    
    private Image healthBG;
    private Material healthBGMaterial;
    
    private TMP_Text healthNumberText;
    
    private float targetHealthNumber = 0;
    private float displayedHealthNumber = 0;

    private void Awake() {
        slider = transform.Find("HealthBarArea/HealthSlider").GetComponent<Slider>();
        healthNumberText = transform.Find("HealthBarArea/HealthSlider/HealthNum").GetComponent<TMP_Text>();
        
        playerModel = this.GetModel<IGamePlayerModel>();

        healthBG = transform.Find("HealthBarArea/HealthSlider/Fill Area/Mask/Fill").GetComponent<Image>();
        healthBGMaterial = Instantiate(healthBG.material);
        healthBG.material = healthBGMaterial;

        targetHealthNumber = playerModel.GetPlayer().HealthProperty.RealValue.Value.CurrentHealth;
        displayedHealthNumber = targetHealthNumber;
        playerModel.GetPlayer().HealthProperty.RealValue.RegisterWithInitValue(OnHealthChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void Update() {
        //lerp displayed health number
        displayedHealthNumber = (int) Mathf.Lerp(displayedHealthNumber, targetHealthNumber, Time.deltaTime / 2);
        healthNumberText.text = displayedHealthNumber + "%";
    }

    private void OnHealthChanged(HealthInfo oldHealth, HealthInfo newHealth) {
        DOTween.Kill(this);
        //int healthNumberAnim = oldHealth.MaxHealth;
        slider.DOValue(newHealth.CurrentHealth / (float) newHealth.MaxHealth, 0.3f);

        DOTween.To(() => targetHealthNumber, x => targetHealthNumber = x, newHealth.CurrentHealth, 0.3f)
            .SetTarget(this);
        
        //lerp material color (becoming redder and redder)
        healthBGMaterial.DOColor(
            Color.Lerp(hurtColor, healthyColor, newHealth.CurrentHealth / (float) newHealth.MaxHealth), 0.3f);
    }
}
