using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Runtime.Enemies.Model.Properties;
using Runtime.Player;
using Runtime.Player.Properties;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarViewController : AbstractMikroController<MainGame> {
    private Slider healthSlider;
    private Slider armorSlider;
    private Slider armorHurtSlider;
    [SerializeField] private float armorHurtSliderWaitTime = 0.5f;
    private float armorHurtSliderWaitTimer = 0;
    private IGamePlayerModel playerModel;
    
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color hurtColor = Color.red;
    
    //[SerializeField] private Color healthyArmorColor = Color.green;
    //[SerializeField] private Color hurtArmorColor = Color.red;
    
    private Image healthBG;
    private Material healthBGMaterial;
    private TMP_Text healthNumberText;
    
    private Image armorBG;
    private Material armorBGMaterial;
    private TMP_Text armorNumberText;
    
    
    private float targetHealthNumber = 0;
    private float displayedHealthNumber = 0;
    
    private float targetArmorNumber = 0;
    private float displayedArmorNumber = 0;

    private float totalArmor = 0;
    private Tween healthNumberTween;
    //private Tween armorNumberTween;
    
    private void Awake() {
        healthSlider = transform.Find("HealthBarArea/HealthSlider").GetComponent<Slider>();
        armorSlider = transform.Find("ArmorArea/ArmorSlider").GetComponent<Slider>();
        armorHurtSlider = transform.Find("ArmorArea/ArmorSliderHurt").GetComponent<Slider>();
        
        healthNumberText = transform.Find("HealthBarArea/HealthSlider/HealthNum").GetComponent<TMP_Text>();
        armorNumberText = transform.Find("ArmorArea/ArmorSlider/ArmorNum").GetComponent<TMP_Text>();
        
        playerModel = this.GetModel<IGamePlayerModel>();

        healthBG = transform.Find("HealthBarArea/HealthSlider/Fill Area/Mask/Fill").GetComponent<Image>();
        healthBGMaterial = Instantiate(healthBG.material);
        healthBG.material = healthBGMaterial;
        
        armorBG = transform.Find("ArmorArea/ArmorSlider/Fill Area/Mask/Fill").GetComponent<Image>();
        armorBGMaterial = Instantiate(armorBG.material);
        armorBG.material = armorBGMaterial;

        targetHealthNumber = playerModel.GetPlayer().HealthProperty.RealValue.Value.CurrentHealth;
        displayedHealthNumber = targetHealthNumber;
        playerModel.GetPlayer().HealthProperty.RealValue.RegisterWithInitValue(OnHealthChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        
        playerModel.GetPlayer().Armor.RegisterWithInitValue(OnArmorChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        
        IArmorProperty maxArmor = playerModel.GetPlayer().GetMaxArmor();
        maxArmor.RealValue.RegisterWithInitValue(OnMaxArmorChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
       
        targetArmorNumber = playerModel.GetPlayer().Armor.Value;
        displayedArmorNumber = targetHealthNumber;
    }

    private void OnMaxArmorChanged(float arg1, float newTotalArmor) {
        totalArmor = newTotalArmor;
    }

    private void OnArmorChanged(float oldArmor, float newArmor) {
        targetArmorNumber = newArmor;
        if (newArmor < oldArmor) {
            armorHurtSliderWaitTimer = armorHurtSliderWaitTime;
        }
        else {
            armorHurtSliderWaitTimer = 0;
        }
    }

    private void Update() {
        //lerp displayed health number
        displayedHealthNumber = Mathf.Lerp(displayedHealthNumber, targetHealthNumber, Time.deltaTime * 3);
        healthNumberText.text = Mathf.RoundToInt(displayedHealthNumber).ToString();
        healthSlider.value = displayedHealthNumber / playerModel.GetPlayer().HealthProperty.RealValue.Value.MaxHealth;

        
        displayedArmorNumber = Mathf.Lerp(displayedArmorNumber, targetArmorNumber,  Time.deltaTime * 3);
        armorHurtSliderWaitTimer -= Time.deltaTime;
        if (armorHurtSliderWaitTimer <= 0) {
            armorHurtSlider.value =
                Mathf.Lerp(armorHurtSlider.value, displayedArmorNumber / totalArmor, Time.deltaTime * 10);
        }
        armorNumberText.text = Mathf.RoundToInt(displayedArmorNumber).ToString();
        armorSlider.value = displayedArmorNumber / totalArmor;
       // armorBGMaterial.color = Color.Lerp(hurtArmorColor, healthyArmorColor, displayedArmorNumber / totalArmor);
    }

    private void OnHealthChanged(HealthInfo oldHealth, HealthInfo newHealth) {
        /*DOTween.Kill(healthSlider);
        healthNumberTween?.Kill();
        
        //int healthNumberAnim = oldHealth.MaxHealth;
        healthSlider.DOValue(newHealth.CurrentHealth / (float) newHealth.MaxHealth, 0.3f);


        healthNumberTween = DOTween.To(() => targetHealthNumber, x => targetHealthNumber = x, newHealth.CurrentHealth,
                0.3f)
            .SetTarget(this).OnComplete(() => {
                healthNumberTween = null;
            });
        
        //lerp material color (becoming redder and redder)
        healthBGMaterial.DOColor(
            Color.Lerp(hurtColor, healthyColor, newHealth.CurrentHealth / (float) newHealth.MaxHealth), 0.3f);*/
        
        
        targetHealthNumber = newHealth.CurrentHealth;
    }
}
