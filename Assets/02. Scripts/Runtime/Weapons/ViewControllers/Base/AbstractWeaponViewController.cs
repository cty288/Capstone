using System;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.GameResources.ViewControllers;
using Runtime.Player;
using Runtime.Temporary.Player;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Builders;
using Runtime.Weapons.Model.Properties;
using UnityEngine;

namespace Runtime.Weapons.ViewControllers.Base
{
    public interface IWeaponViewController : IResourceViewController, ICanDealDamageViewController {
        IWeaponEntity WeaponEntity { get; }
    }
    /// <summary>
    /// For both 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractWeaponViewController<T> : AbstractPickableInHandResourceViewController<T>, IWeaponViewController, IBelongToFaction, IHitResponder
        where T : class, IWeaponEntity, new() {

        private IWeaponModel weaponModel;

        protected IHitDetector hitDetector;

        private bool isScope = false;
        protected IGamePlayerModel playerModel;
        protected override void Awake() {
            base.Awake();
            weaponModel = this.GetModel<IWeaponModel>();
            playerModel = this.GetModel<IGamePlayerModel>();
        }

        protected override void OnEntityStart() {
            base.OnEntityStart();
            hitDetector = OnCreateHitDetector();
            isScope = false;
        }

        public override void OnStartHold(GameObject ownerGameObject) {
            base.OnStartHold(ownerGameObject);
            if(ownerGameObject.TryGetComponent<ICanDealDamageViewController>(out var damageDealer)) {
                BoundEntity.CurrentFaction.Value = damageDealer.CanDealDamageEntity.CurrentFaction.Value;
            }
        }

        public override void OnItemScopePressed() {
            if (playerModel.GetPlayer().GetMovementState() == MovementState.sprinting) {
                return;
            }
            bool previsScope = isScope;
            isScope = OnItemScopePressed(!isScope);
            if (previsScope != isScope) {
                crossHairViewController?.OnScope(isScope);
            }
        }

        public abstract bool OnItemScopePressed(bool shouldScope);

        public override void OnRecycled() {
            base.OnRecycled();
            isScope = false;
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

        public void OnKillDamageable(IDamageable damageable) {
            BoundEntity.OnKillDamageable(damageable);
            crossHairViewController?.OnKill(damageable);
        }

        public void OnDealDamage(IDamageable damageable, int damage) {
            BoundEntity.OnDealDamage(damageable, damage);
            crossHairViewController?.OnHit(damageable, damage);
        }

        public int Damage => BoundEntity.GetBaseDamage().RealValue;
        public bool CheckHit(HitData data) {
            return data.Hurtbox.Owner != gameObject;
        }

        public abstract void HitResponse(HitData data);
        public IWeaponEntity WeaponEntity => BoundEntity;
        public ICanDealDamage CanDealDamageEntity => BoundEntity;
    }
}
