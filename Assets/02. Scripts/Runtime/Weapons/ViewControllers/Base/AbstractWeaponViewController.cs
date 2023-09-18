using System;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.GameResources.ViewControllers;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Builders;
using Runtime.Weapons.Model.Properties;

namespace Runtime.Weapons.ViewControllers.Base
{
    
    /// <summary>
    /// For both 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractWeaponViewController<T> : AbstractPickableInHandResourceViewController<T>, IBelongToFaction, IHitResponder
        where T : class, IWeaponEntity, new() {

        private IWeaponModel weaponModel;

        protected IHitDetector hitDetector;
        
        protected override void Awake() {
            base.Awake();
            weaponModel = this.GetModel<IWeaponModel>();
        }

        protected override void OnEntityStart() {
            base.OnEntityStart();
            hitDetector = OnCreateHitDetector();
        }
        
        
        protected abstract IHitDetector OnCreateHitDetector();

        protected override IEntity OnBuildNewEntity()
        {
            WeaponBuilder<T> builder = weaponModel.GetWeaponBuilder<T>();
            return OnInitWeaponEntity(builder);
        }
        
        protected abstract IEntity OnInitWeaponEntity(WeaponBuilder<T> builder);

        [field: ES3Serializable]
        public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);

        public int Damage => BoundEntity.GetBaseDamage().RealValue;
        public bool CheckHit(HitData data) {
            return data.Hurtbox.Owner != gameObject;
        }

        public abstract void HitResponse(HitData data);
    }
}
