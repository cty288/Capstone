using System;
using System.Collections;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.Serialization;


public class WormBossHeadMine : AbstractBulletViewController
{
    private float speed;
    private float gravity;
    private Vector3 dir;
    private Rigidbody rb;
    private Vector3 target;
    private bool arrived = false;
    private Vector3 randomSpinDir;

    protected override void OnBulletReachesMaxRange()
    {
       
    }

    protected override void OnBulletRecycled()
    {
        
    }

    protected override void OnHitObject(Collider other)
    {
        
    }

    protected override void OnHitResponse(HitData data)
    {
       
    }

    // Start is called before the first frame update
    void Start()
    {
        arrived = false;
        rb = GetComponent<Rigidbody>();
        dir = target - this.transform.position;
        randomSpinDir = UnityEngine.Random.insideUnitSphere;
        randomSpinDir.Normalize();

    }

    // Update is called once per frame
    void Update()
    {
        if (!arrived)
        {

            Vector3 direction = (target - transform.position).normalized;

            // Calculate velocity using constant speed
            Vector3 velocity = direction * 5;

            // Update the bullet's velocity
            this.rb.velocity = velocity;

            // Check if the bullet has reached the target
            if (Vector3.Distance(target, transform.position) < 0.1f)
            {
                arrived = true;
                this.rb.velocity = Vector3.zero;
            }
        }
        else
        {
            transform.Rotate(randomSpinDir, 15 * Time.deltaTime);
        }
        
    }

    internal void SetData(float bulletSpeed, GameObject player, Vector3 target)
    {
        speed = bulletSpeed;
        dir = player.transform.position - this.transform.position;
        this.target = target;
    }

    

}
