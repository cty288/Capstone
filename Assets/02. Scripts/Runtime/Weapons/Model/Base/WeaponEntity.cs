using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Utilities;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using JetBrains.Annotations;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
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
using Runtime.Player;
using Runtime.Temporary;
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
    
    public abstract class ModifyValueEvent{}
    public abstract class ModifyValueEvent<T> : ModifyValueEvent {
        public T Value;
		
        public ModifyValueEvent(T value) {
            Value = value;
        }
    }
    
    public interface IWeaponEntity : IResourceEntity, IHaveCustomProperties, IHaveTags, ICanDealDamage, IHitResponder {
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

        public int GetRarity();
        
        public string DisplayedModelPrefabName { get; }
        
        public BindableProperty<int> CurrentAmmo { get; set; }
        // public GunRecoil GunRecoilScript { get; set; }
        
        public void Reload();

        public void OnRecoil(bool isScopedIn);

        public int GetRealDamageValue();
        
        //public void SetOwner(ICanDealDamage owner);
        
       // public void SetDamageDealer(ICanDealDamage damageDealer);
        public HashSet<WeaponPartsSlot> GetWeaponPartsSlots(WeaponPartType weaponPartType);

        public ReferenceCounter LockWeaponCounter { get; }

        public bool IsLocked { get; }

        public void RegisterOnModifyHitData(Func<HitData, IWeaponEntity, HitData> callback);
        
        public void UnRegisterOnModifyHitData(Func<HitData, IWeaponEntity, HitData> callback);

        public CurrencyType GetMainBuildType();
        
        public int GetTotalBuildRarity(CurrencyType currencyType);

        public Type CurrentBuildBuffType { get; set; }

        public int GetBuildBuffRarityFromBuildTotalRarity(int totalRarity);
        
        public void AddAmmo(int amount);
        // void RegisterOnWeaponPartsUpdate(Action<string, string> callback);
        // void OnModifyHitData(Func<HitData, IWeaponEntity, HitData> onModifyHitData);

        public TEventType SendModifyValueEvent<TEventType>(TEventType modifyValueEvent)
            where TEventType : ModifyValueEvent;

        public void RegisterOnModifyValueEvent<TEventType>(Func<TEventType, TEventType> callback)
            where TEventType : ModifyValueEvent;
        
        public void UnRegisterOnModifyValueEvent<TEventType>(Func<TEventType, TEventType> callback)
            where TEventType : ModifyValueEvent;

        public void ShootUseAmmo(int count);
        
        public void RegisterOnUseAmmo(Action<int> callback);
        
        public void UnRegisterOnUseAmmo(Action<int> callback);
    }

    public struct OnWeaponPartsUpdate {
        public IWeaponEntity WeaponEntity;
        public string PreviousTopPartsUUID;
        public string CurrentTopPartsUUID;
    }


    public interface IFuncRegisterations {
    }

    public class FuncRegisterations<T> : IFuncRegisterations {
        public HashSet<Func<T, T>> OnEvent = new HashSet<Func<T, T>>();
        
        public void Add(Func<T, T> callback) {
            OnEvent.Add(callback);
        }
        
        public void Remove(Func<T, T> callback) {
            OnEvent.Remove(callback);
        }
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

        private Dictionary<Type, IFuncRegisterations> onModifyValueEventCallbacks =
            new Dictionary<Type, IFuncRegisterations>();
        
        
        //protected ICanDealDamageRootEntity rootDamageDealer;
       // protected ICanDealDamage damageDealer;
        
        [field: ES3Serializable]
        public BindableProperty<int> CurrentAmmo { get; set; } = new BindableProperty<int>(0);

        [field: ES3Serializable]
        private Dictionary<WeaponPartType, HashSet<WeaponPartsSlot>> weaponParts = new Dictionary<WeaponPartType, HashSet<WeaponPartsSlot>>();

        //private Action<string, string> onWeaponPartsUpdate;
        private Action<IDamageable, int> onDealDamage;

        private List<Func<HitData, IWeaponEntity, HitData>> onModifyHitData = new List<Func<HitData, IWeaponEntity, HitData>>();
        private Action<ICanDealDamage, IDamageable, int> _onDealDamageCallback;
        private Action<ICanDealDamage, IDamageable> _onKillDamageableCallback;
        private Action<int> _onUseAmmoCallback;
        public abstract int Width { get; }

        protected override ConfigTable GetConfigTable() {
            
            return ConfigDatas.Singleton.WeaponEntityConfigTable;
        }

        public override int GetMaxRarity() {
            return 1;
        }

        public override int GetMinRarity() {
            return 1;
        }

        public override void OnRegisterResourcePropertyDescriptionGetters(ref List<GetResourcePropertyDescriptionGetter> list) {
            base.OnRegisterResourcePropertyDescriptionGetters(ref list);
            
            list.Add(() => new ResourceBuffedPropertyDescription<Vector2Int>(baseDamageProperty,
                null, Localization.Get(
                    "PROPERTY_ICON_DAMAGE"),
                (value) => value.x + " - " + value.y, (initial, real) => real.y - initial.y));


            list.Add(() => new ResourceBuffedPropertyDescription<float>(attackSpeedProperty,
                null,
                Localization.Get("PROPERTY_ICON_ATTACk_SPEED"),
                (value) => Localization.GetFormat("PROPERTY_ICON_ATTACk_SPEED_DESC", value.ToString("f2")),
                (initial, real) => (Math.Abs(real - initial) < 0.01f) ? 0 : (real > initial) ? -1 : 1));

            
            list.Add(() => new ResourceBuffedPropertyDescription<int>(ammoSizeProperty,
                null,
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
                /*AddWeaponPartsSlot(weaponPartType, false);
                AddWeaponPartsSlot(weaponPartType, false);*/
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
            //damageDealer = null;
            CurrentAmmo.UnRegisterAll();
            foreach (KeyValuePair<WeaponPartType,HashSet<WeaponPartsSlot>> part in weaponParts) {
                foreach (WeaponPartsSlot slot in part.Value) {
                    slot.UnregisterOnSlotUpdateCallback(OnWeaponPartSlotUpdate);
                }
            }
            onDealDamage = null;
            onModifyHitData.Clear();
            
            weaponParts.Clear();
            OnModifyDamageCountCallbackList.Clear();
            _onDealDamageCallback = null;
            _onKillDamageableCallback = null;
            damageDealerUUID = null;
            _onUseAmmoCallback = null;
            LockWeaponCounter.Clear();
        }

        public override void OnAddedToInventory(string playerUUID) {
            base.OnAddedToInventory(playerUUID);
            damageDealerUUID = playerUUID;
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

        /*public void SetOwner(ICanDealDamage owner) {
            if (owner != null) {
                ParentDamageDealer = owner;
                CurrentFaction.Value = (this as ICanDealDamage).GetRootDamageDealer().CurrentFaction.Value;
            }
           
        }*/
        

        public HashSet<WeaponPartsSlot> GetWeaponPartsSlots(WeaponPartType weaponPartType) {
            if (!weaponParts.ContainsKey(weaponPartType)) {
                return null;
            }
            return weaponParts[weaponPartType];
        }

        [field: ES3Serializable]
        public ReferenceCounter LockWeaponCounter { get; } = new ReferenceCounter();

        public bool IsLocked => LockWeaponCounter.Count > 0;


        Action<ICanDealDamage, IDamageable, int> ICanDealDamage.OnDealDamageCallback {
            get => _onDealDamageCallback;
            set => _onDealDamageCallback = value;
        }

        Action<ICanDealDamage, IDamageable> ICanDealDamage.OnKillDamageableCallback {
            get => _onKillDamageableCallback;
            set => _onKillDamageableCallback = value;
        }


        public bool CheckHit(HitData data) {
            return true;
        }

        public void HitResponse(HitData data) {
         
        }

        public HitData OnModifyHitData(HitData data) {
            HitData result = data;
            foreach (Func<HitData, IWeaponEntity, HitData> func in onModifyHitData) {
                result = func(result, this);
            }
            return result;
        }

        public void RegisterOnModifyHitData(Func<HitData, IWeaponEntity, HitData> callback) {
            onModifyHitData.Add(callback);
        }

        public void UnRegisterOnModifyHitData(Func<HitData, IWeaponEntity, HitData> callback) {
            onModifyHitData.Remove(callback);
        }

        public CurrencyType GetMainBuildType() {
            if (!weaponParts.ContainsKey(WeaponPartType.Magazine)) {
                return CurrencyType.Time;
            }


            Dictionary<CurrencyType, int> currencyTypes = new Dictionary<CurrencyType, int>();
            foreach (WeaponPartsSlot slot in weaponParts[WeaponPartType.Magazine]) {
                if (slot.GetLastItemUUID() == null) {
                    continue;
                }

                IWeaponPartsEntity weaponPartsEntity = GlobalEntities.GetEntityAndModel(slot.GetLastItemUUID()).Item1 as IWeaponPartsEntity;
                if (weaponPartsEntity == null) {
                    continue;
                }
                

                CurrencyType buildType = weaponPartsEntity.GetBuildType();
                
                if (!currencyTypes.ContainsKey(buildType)) {
                    currencyTypes.Add(buildType, 0);
                }
                currencyTypes[buildType]++;
            }
            
            if (currencyTypes.Count == 0) {
                return CurrencyType.Time;
            }
            
            //get the currency type with the highest count
            int maxCount = 0;
            CurrencyType maxCurrencyType = CurrencyType.Time;
            foreach (KeyValuePair<CurrencyType, int> currencyType in currencyTypes) {
                if (currencyType.Value > maxCount) {
                    maxCount = currencyType.Value;
                    maxCurrencyType = currencyType.Key;
                }
            }

            return maxCurrencyType;
        }

        public int GetBuildBuffRarityFromBuildTotalRarity(int totalRarity) {
            if (totalRarity < 1) {
                return 0;
            }else if (totalRarity < 4) {
                return 1;
            }else if (totalRarity < 7) {
                return 2;
            }
            return 3;
        }

        public void AddAmmo(int amount) {
            int maxAmmo = ammoSizeProperty.RealValue.Value;
            if (CurrentAmmo.Value + amount > maxAmmo) {
                CurrentAmmo.Value = maxAmmo;
            }
            else {
                CurrentAmmo.Value += amount;
            }
        }

        public TEventType SendModifyValueEvent<TEventType>(TEventType modifyValueEvent) where TEventType: ModifyValueEvent {
            Type type = modifyValueEvent.GetType();
            if (!onModifyValueEventCallbacks.ContainsKey(type)) {
                return modifyValueEvent;
            }

            FuncRegisterations<TEventType> registerations =
                onModifyValueEventCallbacks[type] as FuncRegisterations<TEventType>;
            
            
            TEventType result = modifyValueEvent;
            foreach (Func<TEventType, TEventType> func in registerations.OnEvent) {
                result = func(result);
            }

            return result;
        }
        

        public void RegisterOnModifyValueEvent<TEventType>(Func<TEventType, TEventType> callback) where TEventType : ModifyValueEvent {
            Type type = typeof(TEventType);
            if (!onModifyValueEventCallbacks.ContainsKey(type)) {
                onModifyValueEventCallbacks.Add(type, new FuncRegisterations<TEventType>());
            }


            FuncRegisterations<TEventType> registerations =
                onModifyValueEventCallbacks[type] as FuncRegisterations<TEventType>;
            
            registerations.Add(callback);
        }

        public void UnRegisterOnModifyValueEvent<TEventType>(Func<TEventType, TEventType> callback) where TEventType : ModifyValueEvent {
            Type type = typeof(TEventType);
            if (!onModifyValueEventCallbacks.ContainsKey(type)) {
                return;
            }

            FuncRegisterations<TEventType> registerations =
                onModifyValueEventCallbacks[type] as FuncRegisterations<TEventType>;

            registerations.Remove(callback);
        }

        public void ShootUseAmmo(int count) {
            int realCount = Mathf.Min(count, CurrentAmmo.Value);
            CurrentAmmo.Value -= realCount;
            _onUseAmmoCallback?.Invoke(realCount);
        }

        public void RegisterOnUseAmmo(Action<int> callback) {
            _onUseAmmoCallback += callback;
        }

        public void UnRegisterOnUseAmmo(Action<int> callback) {
            _onUseAmmoCallback -= callback;
        }


        public int GetTotalBuildRarity(CurrencyType currencyType) {
            int totalRarity = 0;
            foreach (KeyValuePair<WeaponPartType,HashSet<WeaponPartsSlot>> part in weaponParts) {
                foreach (WeaponPartsSlot slot in part.Value) {
                    if (slot.GetLastItemUUID() == null) {
                        continue;
                    }

                    IWeaponPartsEntity weaponPartsEntity =
                        GlobalGameResourceEntities.GetAnyResource(slot.GetLastItemUUID()) as IWeaponPartsEntity;
                    
                    if (weaponPartsEntity == null) {
                        continue;
                    }

                    if (weaponPartsEntity.GetBuildType() != currencyType) {
                        continue;
                    }

                    totalRarity += weaponPartsEntity.GetRarity();
                }
            }

            return totalRarity;
        }

        [field: ES3Serializable] public Type CurrentBuildBuffType { get; set; } = null;


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
            return true;
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
        public void OnKillDamageable(ICanDealDamage sourceDealer, IDamageable damageable) {
            
        }

        public void OnDealDamage(ICanDealDamage sourceDealer, IDamageable damageable, int damage) {
            Debug.Log($"Dealt {damage} damage to {damageable}");
            onDealDamage?.Invoke(damageable, damage);
        }

        public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; } = new HashSet<Func<int, int>>();

       
        public ICanDealDamage ParentDamageDealer{
            get {
                PlayerController player = PlayerController.GetPlayerByUUID(damageDealerUUID);
                if (player) {
                    return player;
                }
                
                return GlobalEntities.GetEntityAndModel(damageDealerUUID).Item1 as ICanDealDamage;
            }
        }
            
        
        
        [field: ES3Serializable] private string damageDealerUUID { get; set; } = null;
        

        /*ICanDealDamageRootEntity ICanDealDamage.RootDamageDealer => rootDamageDealer;
         public ICanDealDamageRootViewController RootViewController => null;*/
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