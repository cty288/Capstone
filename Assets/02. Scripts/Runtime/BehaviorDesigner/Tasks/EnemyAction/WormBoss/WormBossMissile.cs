using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Spawning;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.AI;
using Runtime.Enemies;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormBossMissile : EnemyAction<WormBossEntity>
    {
        public List<GameObject> missiles = new List<GameObject>();
        private ParticleSystem trail;
        private Rigidbody rb;
        private float timer = 30f;
        // Start is called before the first frame update
        public override void OnStart()
        {
            base.OnStart();
            trail = this.transform.GetChild(1).gameObject.GetComponent<ParticleSystem>();
            trail.Play();
            rb = this.gameObject.GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        public override TaskStatus OnUpdate()
        {
            foreach(var missile in missiles)
            {

                rb.velocity = transform.forward * 5;
            }
            if(timer < 0)
            {
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Running;
            }
            
        }
    }
}

