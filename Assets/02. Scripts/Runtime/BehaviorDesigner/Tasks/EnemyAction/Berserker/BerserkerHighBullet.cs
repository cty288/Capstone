using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using MikroFramework.Pool;
using MikroFramework;
using UnityEngine;

namespace a
{

    public class BerserkerHighBullet : AbstractBulletViewController
    {

        public float bulletSpeed;
        private Transform playerTrans;
        private float timer;
        public GameObject vfx;
        private SafeGameObjectPool pool;
        private GameObject particleInstance;

        // Start is called before the first frame update
        void Start()
        {
            //pool = GameObjectPoolManager.Singleton.CreatePool(vfx, 50, 100);
            timer = Random.Range(0.5f, 2f);
            //Debug.Log(transform.forward);
        }

        // Update is called once per frame
        void Update()
        {
            var dir = (playerTrans.position - this.gameObject.transform.position).normalized;
            var rotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 2f * Time.deltaTime);
            transform.Translate(transform.forward * bulletSpeed * Time.deltaTime, Space.World);
            
        }
        protected override void OnHitResponse(HitData data)
        {

        }

        protected override void OnHitObject(Collider other)
        {
            /*
            Vector3 hitPoint = other.ClosestPointOnBounds(transform.position);
            Vector3 hitNormal = other.ClosestPointOnBounds(transform.position + transform.forward) - transform.position;
            particleInstance = pool.Allocate();
            particleInstance.transform.position = (hitPoint);
            particleInstance.transform.rotation = Quaternion.LookRotation(hitNormal);
            */
        }

        protected override void OnBulletReachesMaxRange()
        {

        }

        protected override void OnBulletRecycled()
        {
            //vfx.SetActive(false);
            timer = Random.Range(0.5f, 2f);


        }
        public void SetData(float bulletSpeed, Transform playerTrans)
        {
            this.bulletSpeed = bulletSpeed;
            this.playerTrans = playerTrans;
        }
    }
}

