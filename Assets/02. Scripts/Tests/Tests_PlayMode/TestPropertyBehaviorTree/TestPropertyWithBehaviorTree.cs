using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Common.Properties;
using BehaviorDesigner.Runtime;
using MikroFramework.BindableProperty;
using UnityEngine;



public class TestEntity : EnemyEntity<TestEntity> {
    [field: SerializeField]
    public override string EntityName { get; protected set; } = "TTT";
    public override void OnRecycle() {
        
    }

    protected override void OnEnemyRegisterProperties() {
        
    }
}


public class TestPropertyWithBehaviorTree : AbstractEnemyViewController<TestEntity> {
    protected override void Awake() {
        base.Awake();
        var entity = entityModel.GetBuilder<TestEntity>().SetProperty(PropertyName.health, new HealthInfo(100, 100))
            .SetProperty(PropertyName.rarity, 1).SetProperty(PropertyName.danger, 100).Build();
        Init(entity.UUID, entity);
    }

    private void Update() {
        BindableProperty<HealthInfo> health = BindedEntity.GetHealth();
        health.Value += 1;
        
        BindableProperty<float> vigiliance = BindedEntity.GetVigiliance();
        vigiliance.Value += 1;
        
        BindableProperty<int> danger = BindedEntity.GetDanger();
        danger.Value += 1;
    }
}
