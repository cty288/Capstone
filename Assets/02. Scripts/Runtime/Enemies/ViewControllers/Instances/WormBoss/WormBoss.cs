using Framework;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.BindableProperty;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.Model.Properties;
using Runtime.Enemies.ViewControllers.Base;
using Runtime.Utilities.ConfigSheet;
using Runtime.Enemies;
using Runtime.UI.NameTags;
using Runtime.Utilities;
using Runtime.Utilities.AnimationEvents;
using Runtime.Utilities.Collision;
using UnityEngine;
using UnityEngine.AI;

using PropertyName = Runtime.DataFramework.Properties.PropertyName;
namespace Runtime.Enemies
{
    public class WormBossEntity : BossEntity<WormBossEntity>
    {
        [field: ES3Serializable]
        public override string EntityName { get; set; } = "WormBoss";
        

        protected override void OnEntityStart(bool isLoadedFromSave)
        {
            Debug.Log("worm start");
        }

        public override void OnRecycle()
        {

        }
        protected override void OnInitModifiers(int rarity, int level)
        {

        }



        protected override void OnEnemyRegisterAdditionalProperties()
        {

        }

        protected override string OnGetDescription(string defaultLocalizationKey)
        {
            return Localization.Get(defaultLocalizationKey);
        }

        protected override ICustomProperty[] OnRegisterCustomProperties()
        {
            
            return new[] {
                new AutoConfigCustomProperty("laserBeam")
                
            };
            
            
        }


    }
    public class WormBoss : AbstractBossViewController<WormBossEntity>
    {
        public int MaxShellHealth { get; }

        public int CurrentShellHealth { get; }
        [Header("HitResponder_Info")]
        public Animator animator;
        public NavMeshAgent agent;



        [SerializeField] private HitBox hitbox_roll;

        [BindCustomData("ranges", "closeRange")]
        public float CloseRange { get; }

        [BindCustomData("ranges", "midRange")]
        public float MidRange { get; }

        [BindCustomData("ranges", "longRange")]
        public float LongRange { get; }

        [BindCustomData("ranges", "meleeRange")]
        public float MeleeRange { get; }

        [BindCustomData("waitTimes", "meleeWait")]
        public float MeleeWait { get; }
        [BindCustomData("waitTimes", "rapidFireWait")]
        public float RapidFireWait { get; }
        [BindCustomData("waitTimes", "rangedAOEWait")]
        public float RangedAOEWait { get; }


        [BindCustomData("waitTimes", "rollWait")]
        public float RollWait { get; }



        private HitDetectorInfo hitDetectorInfo;
        private bool deathAnimationEnd = false;


        [SerializeField]
        private Collider hardCollider;

        [Header("Shell")]
        [SerializeField]
        private Collider shellCollider;

        [SerializeField] private Transform shellHealthBarSpawnTransform;

        [SerializeField] public HitBox slamHitBox;

        [SerializeField] private float meleeKnockbackForce;
        //[SerializeField] private GameObject shellHurbox;
        [SerializeField] private HurtBox[] pedalHurboxes;
        private HashSet<IHurtbox> hashedPedalHurboxes = new HashSet<IHurtbox>();

        private HurtboxModifier _innerShellHurtboxModifier;
        protected override MikroAction WaitingForDeathCondition()
        {
            transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => {
                deathAnimationEnd = true;
            });

            return UntilAction.Allocate(() => deathAnimationEnd);
        }


        protected override void Awake()
        {
            base.Awake();
           
        }

        protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer)
        {

        }

        protected override void OnEntityStart()
        {
        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, ICanDealDamage damagedealer)
        {
            
        }



        protected override IEnemyEntity OnInitEnemyEntity(EnemyBuilder<WormBossEntity> builder)
        {
            return builder.
                FromConfig()
                //.SetAllBasics(0, new HealthInfo(100, 100), TasteType.Type1, TasteType.Type2)
                .Build();
        }

       

        protected override void OnAnimationEvent(string eventName)
        {
            switch (eventName)
            {
                case "ShellOpen":
                    //BoundEntity.IsInvincible.Value = false;
                   
                    // shellHurbox.SetActive(false);
                    break;
                case "ShellClose":
                    //BoundEntity.IsInvincible.Value = true;
                   
                    //shellHurbox.SetActive(true);
                    break;
                case "ClearHits":
                    hitObjects.Clear();
                    break;
                case "MeleeStart":
                    ClearHitObjects();
                    slamHitBox.gameObject.SetActive(true);
                    slamHitBox.StartCheckingHits(BoundEntity.GetCustomDataValue<int>("damages", "meleeDamage").Value);
                    // shellHurbox.SetActive(false);
                    /*foreach (var pedalHurbox in pedalHurboxes) {
                        pedalHurbox.DamageMultiplier = 0;
                    }*/

                    //BoundEntity.ChangeShellStatus(false);
                    _innerShellHurtboxModifier.IgnoreHurtboxCheck = true;
                    break;
                case "MeleeFinish":
                    slamHitBox.StopCheckingHits();
                    slamHitBox.gameObject.SetActive(false);
                    break;
                case "MeleeShellClose":
                    //shellHurbox.SetActive(true);
                    //BoundEntity.ChangeShellStatus(true);
                    _innerShellHurtboxModifier.IgnoreHurtboxCheck = false;
                    break;
                default:
                    break;
            }
        }

       

       


        public void ClearHitObjects()
        {
            hitObjects.Clear();
        }

        public override void HurtResponse(HitData data)
        {
            


        }

        public void ChangeShellStatus(bool newStatus)
        {
            
        }


        public void MeleeAttack()
        {

        }

        public override void HitResponse(HitData data)
        {
            
        }

        public override void OnRecycled()
        {
            
        }
    }
}
