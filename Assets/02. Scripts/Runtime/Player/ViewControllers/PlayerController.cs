using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Commands;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Sandstorm;
using Framework;
using Mikrocosmos;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using MikroFramework.Singletons;
using MikroFramework.Utilities;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Player;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Temporary
{
    public class PlayerController : AbstractCreatureViewController<PlayerEntity>, ISingleton, ICanDealDamage, ICanDealDamageViewController {
        private static HashSet<PlayerController> players = new HashSet<PlayerController>();
        private CameraShaker cameraShaker;
        private TriggerCheck triggerCheck;

        private float recoverWaitTime = 5f;
        protected float recoverWaitTimer = 0f;
        //protected float levelTimer;
        [SerializeField] private AnimationCurve timeCurrencyRate = null;
        //private IPlayerEntity currentPlayerEntity;
        private Coroutine levelTimerCoroutine = null;
        
       // private int levelTimer = 0;
       
        private ILevelModel levelModel;
        private ICurrencySystem currencySystem;
        private Action<ICanDealDamage, IDamageable, int> _onDealDamageCallback;
        private Action<ICanDealDamage, IDamageable> _onKillDamageableCallback;
        private float accumulatedHealthRecover = 0;

        protected override void Awake() {
            base.Awake();
            cameraShaker = GetComponentInChildren<CameraShaker>();
            this.RegisterEvent<OnPlayerTeleport>(OnPlayerTeleport).UnRegisterWhenGameObjectDestroyed(gameObject);
            triggerCheck = transform.Find("GroundCheck").GetComponent<TriggerCheck>();
            levelModel = this.GetModel<ILevelModel>();
            currencySystem = this.GetSystem<ICurrencySystem>();
            levelModel.CurrentLevelCount.RegisterWithInitValue(OnCurrentLevelNumChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<OnSandStormKillPlayer>(OnSandStormKillPlayer)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnSandStormKillPlayer(OnSandStormKillPlayer e) {
            BoundEntity.TakeDamage(Int32.MaxValue, null, out _,null);
        }

        private void OnCurrentLevelNumChanged(int arg1, int levelNum) {
            if (levelTimerCoroutine != null) {
                StopCoroutine(levelTimerCoroutine);
            }

            levelTimerCoroutine = null;
            if (levelNum > 0) {
                levelTimerCoroutine = StartCoroutine(LevelTimer());
            }
        }
        
        private IEnumerator LevelTimer() {
            int levelTime = 0;
            while (true) {
                yield return new WaitForSeconds(1f);
                levelTime++;
                if (levelTime % 30 == 0) {
                    float currency = 0;
                    if(levelTime > timeCurrencyRate.keys[timeCurrencyRate.length - 1].time) {
                        currency = timeCurrencyRate.keys[timeCurrencyRate.length - 1].value;
                    }
                    else {
                        currency = timeCurrencyRate.Evaluate(levelTime);
                    }

                    currencySystem.AddCurrency(CurrencyType.Time, Mathf.RoundToInt(currency));
                }
            }
        }

        private void OnPlayerTeleport(OnPlayerTeleport e) {
            transform.position = e.targetPos;
            triggerCheck.Clear();
        }

        public static PlayerController GetPlayerByUUID(string uuid) {
            foreach (var player in players) {
                if (player.BoundEntity.UUID == uuid) {
                    return player;
                }
            }

            return null;
        }
        public static PlayerController GetClosestPlayer(Vector3 position) {
            if (players.Count == 0) {
                //try find gameobject of type playercontroller
                var playerController = GameObject.FindObjectsOfType<PlayerController>();
                if (playerController.Length == 0) {
                    return null;
                }
                players = new HashSet<PlayerController>(playerController);
            }
            
            
            PlayerController closestPlayer = null;
            float closestDistance = float.MaxValue;

            foreach (var player in players) {
                if (players.Count == 1) {
                    return player;
                }
                float distance = Vector3.Distance(player.transform.position, position);

                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }
            
            return closestPlayer;
        }


        protected override void Update() {
            base.Update();
            if(BoundEntity.GetCurrentHealth() <= 0) {
                return;
            }
            recoverWaitTimer += Time.deltaTime;
            if (recoverWaitTimer >= recoverWaitTime) {
                float armorRecover = BoundEntity.GetArmorRecoverSpeed().RealValue.Value;
                BoundEntity.AddArmor(armorRecover * Time.deltaTime);
                
                float healthRecover = BoundEntity.GetHealthRecoverSpeed().RealValue.Value;
                float lastRecover = accumulatedHealthRecover;
                accumulatedHealthRecover += healthRecover * Time.deltaTime;
                if (Mathf.FloorToInt(accumulatedHealthRecover) > Mathf.FloorToInt(lastRecover)) {
                    BoundEntity.Heal(1, null);
                }
            }

            if (Input.GetKeyDown(KeyCode.C)) {
                //ImageEffectController.Singleton.DisableAllFeatures();
            }

            if (Input.GetKeyDown(KeyCode.F5)) {
                ((SavableArchitecture<MainGame>) MainGame.Interface).SaveGame();
            }
        }

        public static HashSet<PlayerController> GetAllPlayers() {
            if (players.Count == 0) {
                //try find gameobject of type playercontroller
                var playerController = GameObject.FindObjectsOfType<PlayerController>();
                if (playerController.Length == 0) {
                    return null;
                }
                players = new HashSet<PlayerController>(playerController);
            }

            return players;
        }
        protected override IEntity OnBuildNewEntity() {
            return this.GetModel<IGamePlayerModel>().GetPlayer();
        }

        public override ICreature OnInitEntity(int level, int rarity){
            return OnBuildNewEntity() as ICreature;
        }

        protected override void OnEntityStart() {
            Debug.Log("PlayerController.OnEntityStart");
            players.Add(this);
        }

        protected override void OnBindEntityProperty() {
            
        }

        protected override MikroAction WaitingForDeathCondition() {
            return null;
        }

        protected override void OnEntityDie(ICanDealDamage damagedealer) {
            base.OnEntityDie(damagedealer);
        }

        protected override int GetSpawnedCombatCurrencyAmount() {
            return 0;
        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, ICanDealDamage damagedealer) {
            float damageRatio = Mathf.Clamp01((float)damage / (float)BoundEntity.GetMaxHealth());
            
            CameraShakeData shakeData = new CameraShakeData(
                Mathf.Lerp(0f, 5f, damageRatio),
                Mathf.Lerp(0.3f, 0.5f, damageRatio),
                 Mathf.RoundToInt(Mathf.Lerp(30, 70f, damageRatio))
            );
            
            

            cameraShaker.Shake(shakeData, CameraShakeBlendType.Maximum);
            if (damage > 0) {
                recoverWaitTimer = 0f;
            }
        }

        protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer) {
            
        }

        public void OnSingletonInit() {
            
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            players.Remove(this);
        }

        public ICanDealDamage CanDealDamageEntity => BoundEntity;
      
        public Transform GetTransform() {
            return transform;
        }

        public void OnKillDamageable(ICanDealDamage sourceDealer, IDamageable damageable) {
            
        }

        public void OnDealDamage(ICanDealDamage sourceDealer, IDamageable damageable, int damage) {
            
        }

        public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; } = new HashSet<Func<int, int>>();

        Action<ICanDealDamage, IDamageable, int> ICanDealDamage.OnDealDamageCallback {
            get => _onDealDamageCallback;
            set => _onDealDamageCallback = value;
        }

        Action<ICanDealDamage, IDamageable> ICanDealDamage.OnKillDamageableCallback {
            get => _onKillDamageableCallback;
            set => _onKillDamageableCallback = value;
        }

        public ICanDealDamage ParentDamageDealer => BoundEntity;
    }
}
