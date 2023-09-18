using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using UnityEngine;

public abstract class HealthBar : AbstractMikroController<MainGame> {
   public abstract void OnSetEntity(IDamageable entity);
}
