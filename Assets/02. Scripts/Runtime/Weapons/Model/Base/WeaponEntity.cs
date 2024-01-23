using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Inventory.Model;
using Runtime.Utilities.Collision;
using Runtime.Utilities.ConfigSheet;
using Runtime.Weapons.Model.Properties;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Range = Runtime.Weapons.Model.Properties.Range;

namespace Runtime.Weapons.Model.Base
{
    public struct OnWeaponRecoilEvent
    {
        public Vector3 recoilVector;
        public float snappiness;
        public float returnSpeed;
    }
    
    public interface IWeaponEntity : IResourceEntity, IHaveCustomProperties, IHaveTags, ICanDealDamage {
        public IBaseDamage GetBaseDamage();
        public IAttackSpeed GetAttackSpeed();
        public IAdsFOV GetAdsFOV();
        public IRange GetRange();
        public IAmmoSize GetAmmoSize();
        public IReloadSpeed GetReloadSpeed();
        public IBulletsPerShot GetBulletsPerShot();
        public ISpread GetSpread();
        public IRecoil GetRecoil();
        public IScopeRecoil GetScopeRecoil();
        public IBulletSpeed GetBulletSpeed();
        public IChargeSpeed GetChargeSpeed();
        public IWeight GetWeight();
        
        public string DisplayedModelPrefabName { get; }
        
        public BindableProperty<int> CurrentAmmo { get; set; }
        // public GunRecoil GunRecoilScript { get; set; }
        
        public void Reload();

        public void OnRecoil(bool isScopedIn);

        public int GetRealDamageValue();
        
        public void SetRootDamageDealer(ICanDealDamageRootEntity rootDamageDealer);
        public HashSet<WeaponPartsSlot> GetWeaponPartsSlots(WeaponPartType weaponPartType);

        public void RegisterOnDealDamage(Action<IDamageable, int> callback);

        public void UnRegisterOnDealDamage(Action<IDamageable, int> callback);

        public HitData OnModifyHitData(HitData data);

        public void RegisterOnModifyHitData(Func<HitData, HitData> callback);
        
        public void UnRegisterOnModifyHitData(Func<HitData, HitData> callback);
        // void RegisterOnWeaponPartsUpdate(Action<string, string> callback);
    }

    public struct OnWeaponPartsUpdate {
        public IWeaponEntity WeaponEntity;
        public string PreviousTopPartsUUID;
        public string CurrentTopPartsUUID;
    }
    
    public abstract class WeaponEntity<T> :  BuildableResourceEntity<T>, IWeaponEntity  where T : WeaponEntity<T>, new() {
        private IBaseDamage baseDamageProperty;
        private IAttackSpeed attackSpeedProperty;
        private IAdsFOV adsFOVProperty;
        private IRange rangeProperty;
        private IAmmoSize ammoSizeProperty;
        private IReloadSpeed reloadSpeedProperty;
        private IBulletsPerShot bulletsPerShotProperty;
        private ISpread spreadProperty;
        private IRecoil recoilProperty;
        private IScopeRecoil scopeRecoilProperty;
        private IBulletSpeed bulletSpeedProperty;
        private IChargeSpeed chargeSpeedProperty;
        private IWeight weightProperty;
        protected ICanDealDamageRootEntity rootDamageDealer;
        
        [field: ES3Serializable]
        public BindableProperty<int> CurrentAmmo { get; set; } = new BindableProperty<int>(0);

        [field: ES3Serializable]
        private Dictionary<WeaponPartType, HashSet<WeaponPartsSlot>> weaponParts = new Dictionary<WeaponPartType, HashSet<WeaponPartsSlot>>();

        //private Action<string, string> onWeaponPartsUpdate;
        private Action<IDamageable, int> onDealDamage;

        private List<Func<HitData, HitData>> onModifyHitData = new List<Func<HitData, HitData>>();
        public abstract int Width { get; }

        protected override ConfigTable GetConfigTable() {
            
            return ConfigDatas.Singleton.WeaponEntityConfigTable;
        }

        public override int GetMaxRarity() {
            return 1;
        }

        public override void OnRegisterResourcePropertyDescriptionGetters(ref List<GetResourcePropertyDescriptionGetter> list) {
            base.OnRegisterResourcePropertyDescriptionGetters(ref list);
            
            list.Add(() => new ResourceBuffedPropertyDescription<Vector2Int>(baseDamageProperty,
                "PropertyIconDamage", Localization.Get(
                    "PROPERTY_ICON_DAMAGE"),
                (value) => value.x + " - " + value.y, (initial, real) => real.y - initial.y));


            list.Add(() => new ResourceBuffedPropertyDescription<float>(attackSpeedProperty,
                "PropertyIconAttackSpeed",
                Localization.Get("PROPERTY_ICON_ATTACk_SPEED"),
                (value) => Localization.GetFormat("PROPERTY_ICON_ATTACk_SPEED_DESC", value.ToString("f2")),
                (initial, real) => (Math.Abs(real - initial) < 0.01f) ? 0 : (real > initial) ? -1 : 1));

            
            list.Add(() => new ResourceBuffedPropertyDescription<int>(ammoSizeProperty,
                "PropertyIconAmmo",
                Localization.Get("PROPERTY_ICON_AMMO"),
                (val) => val.ToString(),
                (initial, real) => real - initial));
        }
        

        public override ResourceCategory GetResourceCategory() {
            return ResourceCategory.Weapon;
        }

        protected override void OnEntityStart(bool isLoadedFromSave) {
            if (!isLoadedFromSave) { //otherwise it is managed by es3
                CurrentAmmo.Value = ammoSizeProperty.RealValue.Value;
                InitWeaponPartsSlots();
               
                
            }
            foreach (KeyValuePair<WeaponPartType,HashSet<WeaponPartsSlot>> part in weaponParts) {
                foreach (WeaponPartsSlot slot in part.Value) {
                    slot.RegisterOnSlotUpdateCallback(OnWeaponPartSlotUpdate);
                    UpdateWeaponPartsOfType(slot);
                }
            }
        }

        private void UpdateWeaponPartsOfType(WeaponPartsSlot weaponPartsSlot) {
            foreach (WeaponPartsSlot s in weaponParts[weaponPartsSlot.WeaponPartType]) {
                s.UpdateAllWeaponPartsOfThisType(weaponPartsSlot);
            }
        }
        
        protected virtual void OnWeaponPartSlotUpdate(ResourceSlot slot, string previousTopPartsUUID, 
            string currentTopPartsUUID, List<string> uuidList) {

            WeaponPartsSlot weaponPartsSlot = slot as WeaponPartsSlot;
            if (weaponPartsSlot == null) {
                return;
            }

            
            UpdateWeaponPartsOfType(weaponPartsSlot);
            
            this.SendEvent<OnWeaponPartsUpdate>(new OnWeaponPartsUpdate() {
               WeaponEntity = this,
                PreviousTopPartsUUID = previousTopPartsUUID,
                CurrentTopPartsUUID = currentTopPartsUUID
           });
        }
        
       

        protected virtual void InitWeaponPartsSlots() {
            foreach (var t in Enum.GetValues(typeof(WeaponPartType))) {
                WeaponPartType weaponPartType = (WeaponPartType) t;
                if(weaponParts.ContainsKey(weaponPartType)) continue;
                weaponParts.Add(weaponPartType, new HashSet<WeaponPartsSlot>());
                AddWeaponPartsSlot(weaponPartType, false);
                //AddWeaponPartsSlot(weaponPartType, false);
            }
        }

        protected void AddWeaponPartsSlot(WeaponPartType type, bool registerEvent = true) {
            WeaponPartsSlot slot = new WeaponPartsSlot(type, weaponParts[type].Count);
            weaponParts[type].Add(slot);
            if (registerEvent) {
                slot.RegisterOnSlotUpdateCallback(OnWeaponPartSlotUpdate);
            }
            
        }

        public override void OnResourceAwake() {
            base.OnResourceAwake();
            baseDamageProperty = GetProperty<IBaseDamage>();
            attackSpeedProperty = GetProperty<IAttackSpeed>();
            rangeProperty = GetProperty<IRange>();
            ammoSizeProperty = GetProperty<IAmmoSize>();
            reloadSpeedProperty = GetProperty<IReloadSpeed>();
            bulletsPerShotProperty = GetProperty<IBulletsPerShot>();
            spreadProperty = GetProperty<ISpread>();
            recoilProperty = GetProperty<IRecoil>();
            scopeRecoilProperty = GetProperty<IScopeRecoil>();
            bulletSpeedProperty = GetProperty<IBulletSpeed>();
            chargeSpeedProperty = GetProperty<IChargeSpeed>();
            weightProperty = GetProperty<IWeight>();
        }

        public override void OnDoRecycle() {
            SafeObjectPool<T>.Singleton.Recycle(this as T);
        }

        public override void OnRecycle() {
            rootDamageDealer = null;
            CurrentAmmo.UnRegisterAll();
            foreach (KeyValuePair<WeaponPartType,HashSet<WeaponPartsSlot>> part in weaponParts) {
                foreach (WeaponPartsSlot slot in part.Value) {
                    slot.UnregisterOnSlotUpdateCallback(OnWeaponPartSlotUpdate);
                }
            }
            onDealDamage = null;
            onModifyHitData.Clear();
            
            weaponParts.Clear();
            
        }

        protected override void OnEntityRegisterAdditionalProperties() {
            base.OnEntityRegisterAdditionalProperties();
            RegisterInitialProperty<IBaseDamage>(new BaseDamage());
            RegisterInitialProperty<IAttackSpeed>(new AttackSpeed());
            RegisterInitialProperty<IAdsFOV>(new AdsFOV());
            RegisterInitialProperty<IRange>(new Range());
            RegisterInitialProperty<IAmmoSize>(new AmmoSize());
            RegisterInitialProperty<IReloadSpeed>(new ReloadSpeed());
            RegisterInitialProperty<IBulletsPerShot>(new BulletsPerShot());
            RegisterInitialProperty<ISpread>(new Spread());
            RegisterInitialProperty<IRecoil>(new Recoil());
            RegisterInitialProperty<IScopeRecoil>(new ScopeRecoil());
            RegisterInitialProperty<IBulletSpeed>(new BulletSpeed());
            RegisterInitialProperty<IChargeSpeed>(new ChargeSpeed());
            RegisterInitialProperty<IWeight>(new Weight());
        }
        
        public IBaseDamage GetBaseDamage()
        {
            return baseDamageProperty;
        }
        
        public IAttackSpeed GetAttackSpeed()
        {
            return attackSpeedProperty;
        }
        
        public IRange GetRange()
        {
            return rangeProperty;
        }
        
        public IAdsFOV GetAdsFOV()
        {
            return adsFOVProperty;
        }
        
        public IAmmoSize GetAmmoSize()
        {
            return ammoSizeProperty;
        }
        
        public IReloadSpeed GetReloadSpeed()
        {
            return reloadSpeedProperty;
        }
        
        public IBulletsPerShot GetBulletsPerShot()
        {
            return bulletsPerShotProperty;
        }
        
        public ISpread GetSpread()
        {
            return spreadProperty;
        }
        
        public IRecoil GetRecoil()
        {
            return recoilProperty;
        }

        public IScopeRecoil GetScopeRecoil()
        {
            return scopeRecoilProperty;
        }

        public IBulletSpeed GetBulletSpeed()
        {
            return bulletSpeedProperty;
        }
        
        public IChargeSpeed GetChargeSpeed()
        {
            return chargeSpeedProperty;
        }
        
        public IWeight GetWeight()
        {
            return weightProperty;
        }

        public virtual string DisplayedModelPrefabName => $"{EntityName}_Model";

        public int GetRealDamageValue() {
            return Random.Range(baseDamageProperty.RealValue.Value.x, baseDamageProperty.RealValue.Value.y + 1);
        }

        public void SetRootDamageDealer(ICanDealDamageRootEntity rootDamageDealer) {
            this.rootDamageDealer = rootDamageDealer;
            CurrentFaction.Value = rootDamageDealer.CurrentFaction.Value;
        }

        public HashSet<WeaponPartsSlot> GetWeaponPartsSlots(WeaponPartType weaponPartType) {
            if (!weaponParts.ContainsKey(weaponPartType)) {
                return null;
            }
            return weaponParts[weaponPartType];
        }

        public void RegisterOnDealDamage(Action<IDamageable, int> callback) {
            onDealDamage += callback;
        }

        public void UnRegisterOnDealDamage(Action<IDamageable, int> callback) {
            onDealDamage -= callback;
        }

        public HitData OnModifyHitData(HitData data) {
            HitData result = data;
            foreach (Func<HitData, HitData> func in onModifyHitData) {
                result = func(result);
            }
            return result;
        }

        public void RegisterOnModifyHitData(Func<HitData, HitData> callback) {
            onModifyHitData.Add(callback);
        }

        public void UnRegisterOnModifyHitData(Func<HitData, HitData> callback) {
            onModifyHitData.Remove(callback);
        }


        /*public void AddToParts(IWeaponPartsEntity weaponPartsEntity) {
            WeaponPartType weaponPartType = weaponPartsEntity.WeaponPartType;
            HashSet<WeaponPartsSlot> parts = weaponParts[weaponPartType];
            if (parts.Count < weaponPartsSize[weaponPartType]) {
                parts.Add(new WeaponPartsSlot(weaponPartsEntity));
            }
        }

        public bool CanAddToParts(IWeaponPartsEntity weaponPartsEntity) {
            if (weaponPartsEntity == null) {
                return false;
            }

            //check if the number of parts in the list is less than the capacity
            HashSet<string> parts = weaponParts[weaponPartsEntity.WeaponPartType];
            return parts.Count < weaponPartsSize[weaponPartsEntity.WeaponPartType];
        }

        public void RemoveFromParts(string weaponPartID, WeaponPartType weaponPartType) {
            HashSet<string> parts = weaponParts[weaponPartType];
            if (parts.Contains(weaponPartID)) {
                parts.Remove(weaponPartID);
            }
        }*/

        public override bool OnValidateBuff(IBuff buff) {
            if (buff is IWeaponPartsBuff partsBuff) {
                if (partsBuff.WeaponPartsEntity != null) {
                    /*if (weaponParts[partsBuff.WeaponPartsEntity.WeaponPartType].Any((slot =>
                                slot.GetLastItemUUID() == partsBuff.WeaponPartsEntity.UUID))) {
                        return false;
                    }*/
                    return true;
                }
            }

            return false;
        }

        public void Reload() {
            CurrentAmmo.Value = ammoSizeProperty.RealValue.Value;
        }

        public void OnRecoil(bool isScopedIn)
        {
            if (isScopedIn)
            {
                this.SendEvent<OnWeaponRecoilEvent>(new OnWeaponRecoilEvent()
                {
                    recoilVector = scopeRecoilProperty.GetRecoilVector(),
                    snappiness = scopeRecoilProperty.GetSnappiness(),
                    returnSpeed = scopeRecoilProperty.GetReturnSpeed(),
                });
            } else {
                this.SendEvent<OnWeaponRecoilEvent>(new OnWeaponRecoilEvent()
                {
                    recoilVector = recoilProperty.GetRecoilVector(),
                    snappiness = recoilProperty.GetSnappiness(),
                    returnSpeed = recoilProperty.GetReturnSpeed(),
                });
            }
        }

        protected override string OnGetDisplayNameBeforeFirstPicked(string originalDisplayName) {
            return originalDisplayName;
        }

        [field: ES3Serializable] public BindableProperty<Faction> CurrentFaction { get; protected set; } = new BindableProperty<Faction>(Faction.Friendly);
        public void OnKillDamageable(IDamageable damageable) {
            
        }

        public void OnDealDamage(IDamageable damageable, int damage) {
            Debug.Log($"Dealt {damage} damage to {damageable}");
            onDealDamage?.Invoke(damageable, damage);
        }

         ICanDealDamageRootEntity ICanDealDamage.RootDamageDealer => rootDamageDealer;
         public ICanDealDamageRootViewController RootViewController => null;
         public override IResourceEntity GetReturnToBaseEntity() {
             //remove all weapon parts
                foreach (KeyValuePair<WeaponPartType,HashSet<WeaponPartsSlot>> part in weaponParts) {
                    foreach (WeaponPartsSlot slot in part.Value) {
                        string uuid = slot.GetLastItemUUID();
                        slot.Clear();
                        
                        if (uuid != null) {
                            GlobalEntities.GetEntityAndModel(uuid).Item2
                                ?.RemoveEntity(uuid, true);
                        }
                        
                       
                    }
                }
             return this;
         }
    }
}