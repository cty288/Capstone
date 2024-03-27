using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.GameEventSystem.Tests;
using _02._Scripts.Runtime.Levels.Commands;
using _02._Scripts.Runtime.Pillars.Commands;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Skills.Model.Instance;
using _02._Scripts.Runtime.TimeSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Time;
using Framework;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using Runtime.DataFramework.Entities;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Player;
using Runtime.Spawning;
using Runtime.Spawning.ViewControllers.Instances;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace Runtime.Temporary
{
    public class SceneDebug : AbstractMikroController<MainGame>
    {
        

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M)) {
                ICurrencySystem currencySystem = this.GetSystem<ICurrencySystem>();
                currencySystem.AddCurrency(CurrencyType.Combat, 10);
                currencySystem.AddCurrency(CurrencyType.Mineral, 10);
                currencySystem.AddCurrency(CurrencyType.Plant, 10);
                currencySystem.AddCurrency(CurrencyType.Time, 10);
                this.GetModel<ICurrencyModel>().AddMoney(100);
            }
            
            if (!Application.isEditor) {
            //    return;
            }


            if (Input.GetKeyDown(KeyCode.R)) {
                IEnumerable<ResourceTemplateInfo> infos1 =
                    ResourceTemplates.Singleton.GetResourceTemplates(ResourceCategory.WeaponParts);

                IEnumerable<ResourceTemplateInfo> infos2 =
                    ResourceTemplates.Singleton.GetResourceTemplates(ResourceCategory.Weapon,
                        (entity => (entity as IWeaponEntity).GetAttackSpeed().RealValue.Value <= 0.3f));
                
                Debug.Log(infos1.Count());
                Debug.Log(infos2.Count());

                IResourceEntity entity = infos1.First().EntityCreater.Invoke(false, 1);
                Debug.Log(entity.EntityName);
            }


            if (Input.GetKeyDown(KeyCode.P)) {
                IBossPillarViewController[] pillars = FindObjectsOfType<BossPillarViewController>();
                IPillarEntity[] pillarEntities = pillars.Select(pillar => pillar.Entity).ToArray();

                foreach (IPillarEntity entity in pillarEntities) {
                    MainGame.Interface.SendEvent<OnRequestActivatePillar>(new OnRequestActivatePillar() {
                         pillarEntity = entity,
                         level = 1,
                         CurrencyAmount = 999,
                         pillarCurrencyType = CurrencyType.Combat
                    });
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad1)) {
                IPlayerEntity playerEntity = this.GetModel<IGamePlayerModel>().GetPlayer();
                MotivatedBuff buff = BuffPool.FindBuffs((buff) => buff is MotivatedBuff).First()
                    .Invoke(playerEntity, playerEntity, 4) as MotivatedBuff;
                this.GetSystem<IBuffSystem>().AddBuff(playerEntity, playerEntity, buff);
            }
            
            if (Input.GetKeyDown(KeyCode.Keypad2)) {
                IInventoryModel inventoryModel = this.GetModel<IInventoryModel>();
                inventoryModel.RemoveSlots(2, true);
            }
            if (Input.GetKeyDown(KeyCode.Keypad5)) {
                IInventoryModel inventoryModel = this.GetModel<IInventoryModel>();
                inventoryModel.AddSlots(2, out int addedCount);
            }

            if (Input.GetKeyDown(KeyCode.Keypad3)) {
                IInventoryModel inventoryModel = this.GetModel<IInventoryModel>();
                inventoryModel.MaxSlotCount = 100;
                inventoryModel.AddSlots(100, out int addedCount);
                
                IInventorySystem inventorySystem = this.GetSystem<IInventorySystem>();
                var resources = ResourceTemplates.Singleton.GetResourceTemplates((entity) =>
                    entity.GetResourceCategory() == ResourceCategory.WeaponParts && entity.Collectable);
                foreach (var resource in resources) {
                    int minRarity = resource.TemplateEntity is IBuildableResourceEntity resourceEntity      
                        ? resourceEntity.GetMinRarity()
                        : 1;
                    IResourceEntity entity = resource.EntityCreater.Invoke(true, minRarity);
                    inventorySystem.AddItem(entity);
                }
            }
            
            if(Input.GetKeyDown(KeyCode.KeypadPlus)) {
                IGameTimeSystem gameTimeSystem = this.GetSystem<IGameTimeSystem>();
                gameTimeSystem.speed_debug++;
            }
            
            if(Input.GetKeyDown(KeyCode.KeypadMinus)){
                IGameTimeSystem gameTimeSystem = this.GetSystem<IGameTimeSystem>();
                gameTimeSystem.speed_debug--;
            }

            if (Input.GetKeyDown(KeyCode.Keypad4)) {
                var allEntities =GlobalEntities.GetAllEntities();
                StringBuilder sb = new StringBuilder();
                foreach (var entity in allEntities) {
                    sb.AppendLine($"Entity Name: {entity.EntityName}, Display Name: {entity.GetDisplayName()}");
                }
                Debug.Log(sb.ToString());
            }
            
            
            if (Input.GetKeyDown(KeyCode.Keypad6)) {
                var weapons = 
                    ResourceTemplates.Singleton.GetResourceTemplates(ResourceCategory.Weapon,
                         (r)=> r.Collectable);
                
                
                IInventorySystem inventorySystem = this.GetSystem<IInventorySystem>();
                
                
                foreach (var weapon in weapons) {
                    IResourceEntity entity = weapon.EntityCreater.Invoke(true, 1);
                    inventorySystem.AddItem(entity);
                }
            }
            
            
            if (Input.GetKeyDown(KeyCode.Keypad7)) {
                var weapons = 
                    ResourceTemplates.Singleton.GetResourceTemplates(ResourceCategory.Skill,
                        (r)=> r.Collectable);
                
                
                IInventorySystem inventorySystem = this.GetSystem<IInventorySystem>();
                
                
                foreach (var weapon in weapons) {
                    IResourceEntity entity = weapon.EntityCreater.Invoke(true, 1);
                    inventorySystem.AddItem(entity);
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Keypad8)) {
                var weapons = 
                    ResourceTemplates.Singleton.GetResourceTemplates(ResourceCategory.RawMaterial,
                        (r)=> r.Collectable);
                
                
                IInventorySystem inventorySystem = this.GetSystem<IInventorySystem>();
                
                
                foreach (var weapon in weapons) {
                    IResourceEntity entity = weapon.EntityCreater.Invoke(true, 1);
                    inventorySystem.AddItem(entity);
                }
            }

            if (Input.GetKeyDown(KeyCode.N)) {
                this.SendCommand<NextLevelCommand>(NextLevelCommand.Allocate());
            }

            if (Input.GetKeyDown(KeyCode.O)) {
                //ES3.dele
                ((MainGame)MainGame.Interface).ClearSave();
            }

            if (Input.GetKeyDown(KeyCode.U)) {
                IInventoryModel inventoryModel = this.GetModel<IInventoryModel>();
                var uuids = inventoryModel.GetAllItemUUIDs();
                foreach (var uuid in uuids) {
                    IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(uuid);
                    if (entity is IWeaponPartsEntity weaponPartsEntity) {
                        weaponPartsEntity.Upgrade(1);
                        continue;
                    }
                    
                    if(entity is IWeaponEntity weaponEntity) {
                        foreach (var weaponPartType in Enum.GetValues(typeof(WeaponPartType))) {
                            var slots = weaponEntity.GetWeaponPartsSlots((WeaponPartType) weaponPartType);
                            foreach (var slot in slots) {
                                if (slot.GetLastItemUUID() != null) {
                                    IResourceEntity weaponPart = GlobalGameResourceEntities.GetAnyResource(slot.GetLastItemUUID());
                                    if (weaponPart is IWeaponPartsEntity weaponParts) {
                                        weaponParts.Upgrade(1);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
