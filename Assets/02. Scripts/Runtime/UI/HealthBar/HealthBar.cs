using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Enemies.Model.Properties;
using UnityEngine;

public abstract class HealthBar : AbstractMikroController<MainGame> {
   
   
   public abstract void OnSetEntity(IHealthProperty boundHealthProperty, IDamageable entity);
   
   public abstract void OnHealthBarDestroyed();
}
