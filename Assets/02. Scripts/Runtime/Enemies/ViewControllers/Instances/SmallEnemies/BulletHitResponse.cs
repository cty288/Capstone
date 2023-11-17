using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using MikroFramework.Pool;
using MikroFramework;
using UnityEngine;

public class BulletHitResponse : AbstractBulletViewController
{
    public GameObject effect;
    SafeGameObjectPool pool;
    ParticleSystem ps;
   
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
        
        ps = this.gameObject.GetComponent<ParticleSystem>();
    }
    protected override void Update()
    {
        base.Update();
        if(ps.IsAlive(true) == false)
        {
            RecycleToCache();
        }

    }

}
