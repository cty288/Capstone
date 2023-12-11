using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using MikroFramework.Pool;
using MikroFramework;
using UnityEngine;


public class BladeSentinalBladeDanmaku : AbstractBulletViewController
{
    private float bulletSpeed;
    public GameObject particlePrefab;
    private SafeGameObjectPool pool;
    private GameObject particleInstance;
    private GameObject childToRotate;
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
    public void SetData(float bulletSpeed)
    {
        this.bulletSpeed = bulletSpeed;
    }

    // Start is called before the first frame update
    void Start()
    {
        childToRotate = this.gameObject.transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        this.gameObject.GetComponent<Rigidbody>().velocity = this.gameObject.transform.forward * bulletSpeed;
        childToRotate.transform.Rotate(0, 360 * Time.deltaTime, 0);
        this.gameObject.transform.Rotate(0, 90 * Time.deltaTime, 0);
    }
}
