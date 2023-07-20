using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Common.Entities.Enemies;
using _02._Scripts.Runtime.Common.Properties;
using _02._Scripts.Runtime.Common.ViewControllers.Entities.Enemies;
using BehaviorDesigner.Runtime;
using MikroFramework.BindableProperty;
using UnityEngine;



public class TestEntity : EnemyEntity<TestEntity> {
    [field: SerializeField]
    public override string EntityName { get; protected set; } = "TTT";
    public override void OnRecycle() {
        
    }

    protected override void OnEnemyRegisterProperties() {
        RegisterProperty(new CustomProperty());
    }
}

public class CustomProperty : IndependentProperty<int> {
    public override int OnSetBaseValueFromConfig(dynamic value) {
        return value;
    }

    protected override PropertyName GetPropertyName() {
        return PropertyName.test;
    }
}


public class TestPropertyWithBehaviorTree : AbstractEnemyViewController<TestEntity> {
    //in the future, I will make it more easier to use by using [Bind] attribute
    
    [BindableProperty(PropertyName.test, null, nameof(OnTestPropertyChange))]
    public int CustomProperty { get; }


    protected override void OnBindEntityProperty() {
        base.OnBindEntityProperty();
        //Bind("CustomProperty", BindedEntity.GetProperty<CustomProperty>().RealValue);
        //Debug.Log("CustomProperty: " + CustomProperty);
    }

    protected override IEnemyEntity OnInitEnemyEntity(EnemyBuilder<TestEntity> builder) {
        return builder.
           // SetProperty(PropertyName.health, new HealthInfo(100, 100))
           // .SetProperty(PropertyName.danger, 100)
            SetProperty(PropertyName.test, 1000).
            //SetProperty(PropertyName.vigiliance, 10f).
            Build();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.A)) {
            BindableProperty<HealthInfo> health = BindedEntity.GetHealth();
            health.Value += 1;
        
            BindableProperty<float> vigiliance = BindedEntity.GetVigiliance();
            vigiliance.Value += 1;
        
            BindableProperty<int> danger = BindedEntity.GetDanger();
            danger.Value += 1;

            BindableProperty<int> test = BindedEntity.GetProperty<CustomProperty>().RealValue;
            test.Value += 10;
        }

        if (Input.GetKeyDown(KeyCode.D)) {
            Destroy(this.gameObject);
        }
    }
    
    protected void OnTestPropertyChange(int oldValue, int newValue){
        Debug.Log("CustomProperty Changed: " + newValue);
    }
    
    
}
