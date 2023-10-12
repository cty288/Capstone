using Framework;
using MikroFramework.ActionKit;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.Model.Properties;
using Runtime.Enemies.ViewControllers.Base;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.RunTimeTests.TestPropertyBehaviorTree {
    public class TestEntity : EnemyEntity<TestEntity> {
        [field: SerializeField]
        public override string EntityName { get; set; } = "TestEnemy2";
    
        [field: ES3Serializable]
        public int MyPersistentButNotInherentData { get; set; } = 100;
    
        public override void OnRecycle() {
        
        }
        protected override void OnInitModifiers(int rarity) {
            
        }
        public override int OnGetRealSpawnWeight(int level, int baseWeight) {
            return level;
        }

        public override int OnGetRealSpawnCost(int level, int rarity, int baseCost) {
            return level;
        }
        protected override string OnGetDescription(string defaultLocalizationKey) {
            return null;
        }

        protected override ConfigTable GetConfigTable() {
            return ConfigDatas.Singleton.EnemyEntityConfigTable_Test;
        }

        protected override void OnEntityStart(bool isLoadedFromSave) {
            
        }

        protected override void OnEnemyRegisterAdditionalProperties() {
            RegisterInitialProperty(new NewProperty());
        }

        protected override ICustomProperty[] OnRegisterCustomProperties() {
            return new[] {new AutoConfigCustomProperty("attack1"), new AutoConfigCustomProperty("attack2")};
        }

        protected override Faction GetDefaultFaction() {
            return Faction.Neutral;
        }
    }

    public class NewProperty : IndependentProperty<int> {

        protected override PropertyName GetPropertyName() {
            return PropertyName.test;
        }
    }

    public struct TestInfo {
        public float test;
    
        public TestInfo(float test) {
            this.test = test;
        }
    }


    public class TestPropertyWithBehaviorTree : AbstractEnemyViewController<TestEntity> {
    
        [SerializeField]
        protected bool isVariant = false;

        [SerializeField] protected int overrideHealth = 0;
    
        //[Bind(PropertyName.test)]
    
        //[Bind("test", null, nameof(OnTestPropertyChange))]
        public int TestProperty { get; }


        [BindCustomData("attack1", "damage", null,
            nameof(OnAttack1DamageChanged))]
        public int Attack1Damage { get; }
    
        public int Attack1Damage2 { get; }
    
        [BindCustomData("attack1", "info", nameof(GetAttack1Test),
            nameof(OnAttack1TestChanged))]
    
        public float Attack1Test { get; }
    
    
        public float Attack1Test2 { get; }

        protected override void OnEntityStart() {
            Bind("TestProperty", BoundEntity.GetProperty<NewProperty>().RealValue);
        
            BoundEntity.RegisterOnCustomDataChanged("attack1", "damage", OnRegisteredCustomAttack1DamageChanged);

            BindCustomData<int>("Attack1Damage2", "attack1", "damage", OnAttack1DamageChanged2);
        
            BindCustomData
                ("Attack1Test2", "attack1", "info", GetAttack1Test2, OnAttack1TestChanged2);

            BoundEntity.GetCustomDataValue<int>("attack1", "speed");
        }


        protected override void OnAnimationEvent(string eventName) {
            
        }

        protected override HealthBar OnSpawnHealthBar() {
            return null;
        }

        protected override void OnDestroyHealthBar(HealthBar healthBar) {
            
        }

        protected override IEnemyEntity OnInitEnemyEntity(EnemyBuilder<TestEntity> builder) {
            if (!isVariant) {
                return builder.
                    FromConfig().
                    SetProperty(new PropertyNameInfo(PropertyName.test), 1000).
                    Build();
            }
            else {
                return builder.
                    FromConfig().
                    SetProperty(new PropertyNameInfo(PropertyName.health), new HealthInfo(overrideHealth, overrideHealth)).
                    SetProperty(new PropertyNameInfo(PropertyName.test), 1000).
                    Build();
            }

        }

        private void Update() {
        
        
        
            if (Input.GetKeyDown(KeyCode.A)) {
            
                Debug.Log(BoundEntity.MyPersistentButNotInherentData);
                
        
                BindableProperty<int> danger = BoundEntity.GetDanger();
                danger.Value += 1;

                BindableProperty<int> test = BoundEntity.GetProperty<NewProperty>().RealValue;
                test.Value += 10;

                BoundEntity.GetCustomDataValue("attack1", "damage").Value += 1;
                IBindableProperty attack1Prop = BoundEntity.GetCustomDataValue("attack1", "info");
                attack1Prop.Value = new TestInfo(attack1Prop.Value.test + 1);
            }
            
            if(Input.GetKeyDown(KeyCode.G)) {
                BoundEntity.TakeDamage(10, null);
            }
            
            if(Input.GetKeyDown(KeyCode.H)) {
                BoundEntity.Heal(10, null);
            }

            if (Input.GetKeyDown(KeyCode.D)) {
                Destroy(this.gameObject);
            }
            

            if (Input.GetKeyDown(KeyCode.Space)) {
                Debug.Log($"Attack 1 Damage: {Attack1Damage}");
            }

            if (Input.GetKeyDown(KeyCode.R)) {
                enemyModel.RemoveEntity(BoundEntity.UUID);
            }
        }
    
        protected void OnTestPropertyChange(int oldValue, int newValue){
            Debug.Log("TestProperty Changed: " + newValue);
        }

        protected void OnAttack1DamageChanged(int oldValue, int newValue) {
            Debug.Log($"[Bind Attribute] Attack 1 Damage Changed to: {newValue}");
        }
    
        protected void OnAttack1DamageChanged2(int oldValue, int newValue) {
            Debug.Log($"[Bind Function] Attack 1 Damage Changed from {oldValue} to: {newValue}");
        }
    
        protected dynamic GetAttack1Test(dynamic input) {
            return input.test;
        }
        protected void OnAttack1TestChanged(float oldValue, float newValue) {
            Debug.Log($"[Bind Attribute] Attack 1 Test Changed to: {newValue}");
        }
    
        private void OnRegisteredCustomAttack1DamageChanged(ICustomDataProperty property, dynamic oldValue, dynamic newValue) {
            Debug.Log($"Attack 1 Damage Changed to (By OnRegisteredCustomAttack1DamageChanged) : {newValue}");
        }
    
        protected float GetAttack1Test2(dynamic input) {
            return input.test;
        }
    
        protected void OnAttack1TestChanged2(float oldValue, float newValue) {
            Debug.Log($"[Bind Function] Attack 1 Test Changed to: {newValue}");
        }

        protected override void OnEntityDie(IBelongToFaction damagedealer) {
            Debug.Log("TestEntity Die");
        }

        protected override MikroAction WaitingForDeathCondition() {
            return null;
        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, IBelongToFaction damagedealer) {
            Debug.Log($"TestEntity Take Damage, current health : {currenthealth}");
        }

        protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer) {
           Debug.Log("TestEntity Heal, current health : " + currenthealth);
        }
    }
}