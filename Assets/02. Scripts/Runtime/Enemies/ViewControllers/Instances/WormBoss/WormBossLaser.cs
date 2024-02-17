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
    //public GameObject particlePrefab;
    private GameObject particleInstance;

    
    protected override void OnBulletReachesMaxRange()
    {
        //throw new System.NotImplementedException();
    }

    protected override void OnBulletRecycled()
    {
        //throw new System.NotImplementedException();
    }

    protected override void OnHitObject(Collider other)
    {
        //throw new System.NotImplementedException();
    }

    protected override void OnHitResponse(HitData data)
    {
        //throw new System.NotImplementedException();
    }

    // private SafeGameObjectPool pool;
    // Start is called before the first frame update
    void Start()
    {
        player = PlayerController.GetClosestPlayer(transform.position).transform.gameObject;
        this.gameObject.GetComponent<DotHitBox>().dotTick = tick;
    }

    private void OnEnable()
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 100f);
    }

    // Update is called once per frame
//     protected override void Update()
//     {
//         base.Update();
//         transform.localScale += new Vector3(0, 0, 5.5f);
//         /*
//         if (this.gameObject.transform.localScale.z >= 30)
//         {
//             this.gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, 30f);
//
//         }
//         */
//     }
    
    public void SetData(float tick, float damage)
    {
        this.tick = tick;
        this.damage = damage;
    }
    
    /*
    protected override void OnTriggerEnter(Collider other)
    {
        // if (gameObject.name == "GunBullet") {
        // 	Debug.Log("HitResponse");
        // }
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
                if (belongToFaction.CurrentFaction.Value == CurrentFaction.Value && false)
                {
                    return;
                }
            }

            OnHitObject(other);
            //RecycleToCache();
        }
    }
    private void OnTriggerStay(Collider collision)
    {

        if (collision.gameObject.layer == 8 || collision.gameObject.layer == 11)
        {
            if (collision.gameObject.layer == 11)
            {
                if (particleInstance == null)
                {
                    // particleInstance = pool.Allocate();
                }
                Vector3 hitPoint = collision.ClosestPointOnBounds(transform.position);
                Vector3 hitNormal = collision.ClosestPointOnBounds(transform.position + transform.forward) - transform.position;

                // Instantiate the particle system at the hit point with the correct rotation

                //particleInstance.transform.position = (hitPoint);
                //particleInstance.transform.rotation = Quaternion.LookRotation(hitNormal);
            }


           // pause = true;
        }
    }
    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.layer == 8 || collision.gameObject.layer == 11)
        {

            //pause = false;
        }
    }
    */
}
