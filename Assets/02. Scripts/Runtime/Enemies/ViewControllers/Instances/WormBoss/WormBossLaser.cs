using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using System.Collections;
using MikroFramework.Pool;

using MikroFramework.Pool;
using MikroFramework;
using Runtime.Temporary;


public class WormBossLaser : AbstractDotBulletViewController
{
    public GameObject parent;
    public LineRenderer lineRenderer;
    private float damage;
    private float bulletSpeed = 34f;
    private Vector3 dir;
    private GameObject face;
    private float tickrate = 0.1f;
    private float tick = 0.2f;
    private float waitBeforeRotate = 1.5f;
    private float timer = 1.5f;
    private GameObject player;
    private GameObject particleInstance;
    private Vector3 originalLocalScale = new (170f, 170f, 10f);
    
    // Start is called before the first frame update
    void Start()
    {
        player = PlayerController.GetClosestPlayer(transform.position).transform.gameObject;
        gameObject.GetComponent<DotHitBox>().dotTick = tick;
    }
    
    public void SetData(float tick, float damage)
    {
        this.tick = tick;
        this.damage = damage;
        
        transform.localScale = new Vector3(
            originalLocalScale.x, 
            originalLocalScale.y, 
            originalLocalScale.z * maxRange);
    }
    
    protected override void OnBulletReachesMaxRange()
    {
    }

    protected override void OnBulletRecycled()
    {
        transform.localScale = originalLocalScale;
    }

    protected override void OnHitObject(Collider other)
    {
    }

    protected override void OnHitResponse(HitData data)
    {
    }
    
    protected override void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            Rigidbody rootRigidbody = other.attachedRigidbody;
            GameObject hitObj =
                rootRigidbody ? rootRigidbody.gameObject : other.gameObject;

            if (hitObj != null && hitObj.transform == bulletOwner.transform)
            {
                return;
            }
            if (hitObj.TryGetComponent<IBelongToFaction>(out var belongToFaction))
            {
                if (belongToFaction.CurrentFaction.Value == CurrentFaction.Value && penetrateSameFaction)
                {
                    return;
                }
            }

            OnHitObject(other);
        }
    }
}
