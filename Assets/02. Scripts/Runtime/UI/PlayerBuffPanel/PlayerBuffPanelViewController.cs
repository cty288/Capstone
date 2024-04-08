using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities;
using Runtime.Player;
using UnityEngine;

public class PlayerBuffPanelViewController : AbstractMikroController<MainGame> {
   private ResLoader resLoader;
   private Transform buffSpawnParent;
   
   private Dictionary<IBuff, BuffIconViewController> buffToGameObject = new Dictionary<IBuff, BuffIconViewController>();

   [SerializeField] private RectTransform inventoryLayout;
   [SerializeField] private float inventoryLayoutPosYNoBuff = 228;
   [SerializeField] private float inventoryLayoutPosYHasBuff = 287;
   private void Awake() {
      resLoader = this.GetUtility<ResLoader>();
      buffSpawnParent = transform.Find("BuffPanel");
      this.RegisterEvent<OnPlayerBuffUpdate>(OnPlayerBuffUpdate).UnRegisterWhenGameObjectDestroyed(gameObject);
   }

   private void OnPlayerBuffUpdate(OnPlayerBuffUpdate e) {
      BuffDisplayInfo buffDisplayInfo = e.Buff.OnGetBuffDisplayInfo();
      switch (e.EventType) {
         case BuffUpdateEventType.OnStart:
            BuffIconViewController buffIcon = Instantiate(resLoader.LoadSync<GameObject>(buffDisplayInfo.IconPrefab))
               .GetComponent<BuffIconViewController>();
            
            buffIcon.transform.SetParent(buffSpawnParent);
            buffIcon.transform.localScale = Vector3.one;
            buffIcon.SetBuff(e.Buff);
            buffToGameObject.Add(e.Buff, buffIcon);
            buffIcon.OnRefresh();
            break;
         case BuffUpdateEventType.OnUpdate:
            if (buffToGameObject.TryGetValue(e.Buff, out var value)) {
               value.OnRefresh();
            }
            
            break;
         case BuffUpdateEventType.OnEnd:
            if (buffToGameObject.TryGetValue(e.Buff, out var buffIconViewController)) {
               buffToGameObject.Remove(e.Buff);
               Destroy(buffIconViewController.gameObject);
            }
            break;
      }
      
      if (buffToGameObject.Count > 0) {
         inventoryLayout.anchoredPosition = new Vector2(inventoryLayout.anchoredPosition.x, inventoryLayoutPosYHasBuff);
      }
      else {
         inventoryLayout.anchoredPosition = new Vector2(inventoryLayout.anchoredPosition.x, inventoryLayoutPosYNoBuff);
      }
   }
}
