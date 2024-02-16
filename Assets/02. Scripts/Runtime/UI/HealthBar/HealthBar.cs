using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Enemies.Model.Properties;
using UnityEngine;

public abstract class HealthBar : AbstractMikroController<MainGame> {
   
   private Transform buffSpawnParent;
   protected IDamageable entity;
   private Dictionary<IBuff, BuffIconViewController> buffToGameObject = new Dictionary<IBuff, BuffIconViewController>();
   private ResLoader resLoader;
   protected virtual void Awake() {
      buffSpawnParent = transform.Find("BuffPanel");
      resLoader = this.GetUtility<ResLoader>();
   }

   public void SetEntity(BindableProperty<HealthInfo> boundHealthProperty, IDamageable entity) {
      this.entity = entity;
      this.entity.RegisterOnBuffUpdate(OnBuffUpdate);
      OnSetEntity(boundHealthProperty, entity);
   }
   
   protected virtual void OnBuffUpdate(IBuff buff, BuffUpdateEventType updateType) {
      BuffDisplayInfo buffDisplayInfo = buff.OnGetBuffDisplayInfo();
      if (!buffDisplayInfo.Display) {
         return;
      }
      if (resLoader == null) {
        Awake();
      }
      switch (updateType) {
         case BuffUpdateEventType.OnStart:
            BuffIconViewController buffIcon = Instantiate(resLoader.LoadSync<GameObject>(buffDisplayInfo.IconPrefab))
               .GetComponent<BuffIconViewController>();
            
            buffIcon.transform.SetParent(buffSpawnParent);
            buffIcon.transform.localScale = Vector3.one;
					
            //get height of the spawn parent
            float height = buffSpawnParent.GetComponent<RectTransform>().rect.height;
            //set width/height of the buff icon
            buffIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(height, height);
					
            buffIcon.SetBuff(buff);
            if (!buffToGameObject.TryAdd(buff, buffIcon)) {
               Destroy(buffIcon.gameObject);
            }
            buffIcon.OnRefresh();
            break;
         case BuffUpdateEventType.OnUpdate:
            if (buffToGameObject.TryGetValue(buff, out var value)) {
               value.OnRefresh();
            }
            
            break;
         case BuffUpdateEventType.OnEnd:
            if (buffToGameObject.TryGetValue(buff, out var buffIconViewController)) {
               buffToGameObject.Remove(buff);
               Destroy(buffIconViewController.gameObject);
            }
            break;
      }
   }

   
   protected abstract void OnSetEntity(BindableProperty<HealthInfo> boundHealthProperty, IDamageable entity);
   
   
   public void DestroyHealthBar() {
      foreach (var buffIcon in buffToGameObject.Values) {
         Destroy(buffIcon.gameObject);
      }
			
      buffToGameObject.Clear();
      OnHealthBarDestroyed();
   }
   
   protected abstract void OnHealthBarDestroyed();
}
