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
    private void Awake()
    {
        //pool = GameObjectPoolManager.Singleton.CreatePool(effect, 30, 50);
    }
    protected override void OnBulletReachesMaxRange()
    {
        throw new System.NotImplementedException();
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

    // Update is called once per frame
    void Update()
    {
        if(ps.IsAlive(true) == false)
        {
            if(Pool != null)
            {
                RecycleToCache();
            }
                
        }
    }
}
