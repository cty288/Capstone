using System;
using System.Collections;
using System.Collections.Generic;
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
    protected override PropertyName GetPropertyName() {
        return PropertyName.test;
    }
}


public class TestPropertyWithBehaviorTree : AbstractEnemyViewController<TestEntity> {
    //in the future, I will make it more easier to use by using [Bind] attribute
    
    [BindableProperty(PropertyName.test)]
    public int CustomProperty { get; set; }
    protected override void Awake() {
        base.Awake();
        var entity = entityModel.GetBuilder<TestEntity>().SetProperty(PropertyName.health, new HealthInfo(100, 100))
            .SetProperty(PropertyName.rarity, 1).SetProperty(PropertyName.danger, 100)
            .SetProperty(PropertyName.test, 1000).
            Build();
        Init(entity.UUID, entity);
    }

    protected override void OnBindEntityProperty() {
        base.OnBindEntityProperty();
        //Bind("CustomProperty", BindedEntity.GetProperty<CustomProperty>().RealValue);
        //Debug.Log("CustomProperty: " + CustomProperty);
    }

    private void Update() {
        Debug.Log("CustomProperty: " + CustomProperty);
        BindableProperty<HealthInfo> health = BindedEntity.GetHealth();
        health.Value += 1;
        
        BindableProperty<float> vigiliance = BindedEntity.GetVigiliance();
        vigiliance.Value += 1;
        
        BindableProperty<int> danger = BindedEntity.GetDanger();
        danger.Value += 1;
        
    }
}
