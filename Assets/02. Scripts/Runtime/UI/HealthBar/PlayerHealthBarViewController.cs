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
    private IGamePlayerModel playerModel;
    
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color hurtColor = Color.red;
    
    [SerializeField] private Color healthyArmorColor = Color.green;
    [SerializeField] private Color hurtArmorColor = Color.red;
    
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
        
        IArmorProperty armorProperty = playerModel.GetPlayer().GetArmor();
        armorProperty.RealValue.RegisterWithInitValue(OnArmorChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        totalArmor = armorProperty.InitialValue;
        targetArmorNumber = armorProperty.RealValue.Value;
        displayedArmorNumber = targetHealthNumber;
    }

    private void OnArmorChanged(float oldArmor, float newArmor) {
        /*
        DOTween.Kill(armorSlider);
        armorNumberTween?.Kill();
        
        armorSlider.DOValue(newArmor / totalArmor, 0.3f);
        
        //lerp material color (becoming redder and redder)
        armorBGMaterial.DOColor(
            Color.Lerp(hurtArmorColor, healthyArmorColor, newArmor / totalArmor), 0.1f);

        armorNumberTween = DOTween.To(() => targetArmorNumber, x => targetArmorNumber = x, newArmor, 0.1f).OnComplete(
            () => {
                armorNumberTween = null;
            });
            */
        
        targetArmorNumber = newArmor;

    }

    private void Update() {
        //lerp displayed health number
        displayedHealthNumber = (int) Mathf.Lerp(displayedHealthNumber, targetHealthNumber, Time.deltaTime / 2);
        healthNumberText.text = displayedHealthNumber.ToString();

        displayedArmorNumber = Mathf.Lerp(displayedArmorNumber, targetArmorNumber, 1f / 3 / Time.deltaTime);
        armorNumberText.text = Mathf.RoundToInt(displayedArmorNumber).ToString();
        armorSlider.value = displayedArmorNumber / totalArmor;
        armorBGMaterial.color = Color.Lerp(hurtArmorColor, healthyArmorColor, displayedArmorNumber / totalArmor);
    }

    private void OnHealthChanged(HealthInfo oldHealth, HealthInfo newHealth) {
        DOTween.Kill(healthSlider);
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
            Color.Lerp(hurtColor, healthyColor, newHealth.CurrentHealth / (float) newHealth.MaxHealth), 0.3f);
    }
}
