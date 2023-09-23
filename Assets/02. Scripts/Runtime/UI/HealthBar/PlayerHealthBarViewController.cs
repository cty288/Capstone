using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Runtime.Enemies.Model.Properties;
using Runtime.Player;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarViewController : AbstractMikroController<MainGame> {
    private Slider slider;
    private IGamePlayerModel playerModel;
    private void Awake() {
        slider = transform.Find("HealthBarArea/HealthSlider").GetComponent<Slider>();
        
        playerModel = this.GetModel<IGamePlayerModel>();

        playerModel.GetPlayer().HealthProperty.RealValue.RegisterWithInitValue(OnHealthChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void OnHealthChanged(HealthInfo oldHealth, HealthInfo newHealth) {
        slider.DOValue(newHealth.CurrentHealth / (float) newHealth.MaxHealth, 0.3f);
    }
}
