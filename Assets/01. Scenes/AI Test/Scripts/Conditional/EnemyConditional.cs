using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class EnemyConditional : Conditional
{
    protected Rigidbody2D body;
    protected Animator animator;
    public SharedBool condition;
    //further implementation
    //protected Destructable destructable;
    //protected PlayerController player;

    public override void OnAwake()
    {

        body = GetComponent<Rigidbody2D>();
        //player = PlayerController.Instance;
        //destructable = GetComponent<Destructable>();
        animator = gameObject.GetComponentInChildren<Animator>();
    }

    public override TaskStatus OnUpdate()
    {
        if (condition.Value) return TaskStatus.Success;
        else return TaskStatus.Failure;
    }
}