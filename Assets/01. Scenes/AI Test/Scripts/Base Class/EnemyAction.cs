using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

public class EnemyAction : Action
{
    protected Rigidbody body;
    protected Animator anim;
    

    public override void OnAwake()
    {
        body = GetComponent<Rigidbody>();
        anim = gameObject.GetComponent<Animator>();
        
    }

}
