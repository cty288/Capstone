using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class IsHealthUnder : EnemyConditional
{
    public SharedFloat healthPercentage;
    public TestEnity boss; // this boss should be whatever entity script or instance to be changed later

    public SharedBool checkOnce = false;
    public bool activated = false;

    public override TaskStatus OnUpdate()
    {
        // if (checkOnce.Value && activated)
        // {
        //     return TaskStatus.Failure;
        // }

        // if (!activated)
        // {
        //     int curHealth = boss.curHealth;
        //     int maxHealth = boss.maxHealth;
        //     if (curHealth < maxHealth * healthPercentage.Value)
        //     {
        //         Debug.Log(curHealth);
        //         if (checkOnce.Value)
        //         {
        //             activated = true;
        //         }
        //         return TaskStatus.Success;
        //     }
        // }
        int curHealth = boss.curHealth;
        int maxHealth = boss.maxHealth;
        if (curHealth < maxHealth * healthPercentage.Value)
        {
            Debug.Log(curHealth);
            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
    }



}
