using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Common.Entities.Enemies;
using _02._Scripts.Runtime.Common.Properties;
using _02._Scripts.Runtime.Common.Properties.SkillsBase;
using _02._Scripts.Runtime.Common.ViewControllers.Entities.Enemies;
using BehaviorDesigner.Runtime;
using JetBrains.Annotations;
using MikroFramework.BindableProperty;
using UnityEngine;



public class TestEntity : EnemyEntity<TestEntity> {
    [field: SerializeField]
    public override string EntityName { get; protected set; } = "TestEnemy2";
    
    [field: ES3Serializable]
    public int MyPersistentButNotInherentData { get; set; } = 100;
    
    public override void OnRecycle() {
        
    }

    protected override void OnEnemyRegisterProperties() {
        RegisterInitialProperty(new NewProperty());
    }

    protected override ICustomProperty[] OnRegisterCustomProperties() {
        return new[] {new AutoConfigCustomProperty("attack1"), new AutoConfigCustomProperty("attack2")};
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
        Bind("TestProperty", BindedEntity.GetProperty<NewProperty>().RealValue);
        
        BindedEntity.RegisterOnCustomDataChanged("attack1", "damage", OnRegisteredCustomAttack1DamageChanged);

        BindCustomData<int>("Attack1Damage2", "attack1", "damage", OnAttack1DamageChanged2);
        
        BindCustomData
            ("Attack1Test2", "attack1", "info", GetAttack1Test2, OnAttack1TestChanged2);

        BindedEntity.GetCustomDataValue<int>("attack1", "speed");
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
            
            Debug.Log(BindedEntity.MyPersistentButNotInherentData);
            
            BindableProperty<HealthInfo> health = BindedEntity.GetHealth();
            health.Value += 1;
        
            BindableProperty<float> vigiliance = BindedEntity.GetVigiliance();
            vigiliance.Value += 1;
        
            BindableProperty<int> danger = BindedEntity.GetDanger();
            danger.Value += 1;

            BindableProperty<int> test = BindedEntity.GetProperty<NewProperty>().RealValue;
            test.Value += 10;

            BindedEntity.GetCustomDataValue("attack1", "damage").Value += 1;
            IBindableProperty attack1Prop = BindedEntity.GetCustomDataValue("attack1", "info");
            attack1Prop.Value = new TestInfo(attack1Prop.Value.test + 1);
        }

        if (Input.GetKeyDown(KeyCode.D)) {
            Destroy(this.gameObject);
        }

        if (Input.GetKeyDown(KeyCode.S)) {
            ES3AutoSaveMgr.Current.Save();
            ((MainGame) MainGame.Interface).SaveGame();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log($"Attack 1 Damage: {Attack1Damage}");
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

}
