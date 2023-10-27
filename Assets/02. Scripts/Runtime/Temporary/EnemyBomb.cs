using System.Collections;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace Runtime.Temporary
{
    public class EnemyBomb : AbstractBulletViewController
    {
        public AnimationCurve curve;
        private Vector3 targetPos;
        public float travelTime;
        private Vector3 start;


        public GameObject explosion;
        private int explosionDamage;
        private float explosionSize;
      
        void Start() {

        }

        // Update is called once per frame
        void Update()
        {

        }

        protected override void OnBulletReachesMaxRange() {
            
        }

        public void Init(Transform target,float tTime, Faction faction, int damage, GameObject bulletOwner) {
            Init(faction, 0, bulletOwner, bulletOwner.GetComponent<ICanDealDamage>(), -1);
            targetPos = target.position;
            explosionDamage = damage;
            travelTime = tTime;
            start = transform.position;
            StartCoroutine(Curve());
        }
        public void Init(Vector3 target, float tTime, Faction faction, int damage, float size ,GameObject bulletOwner)
        {
            Init(faction, 0, bulletOwner, bulletOwner.GetComponent<ICanDealDamage>(), -1);
            targetPos = target ;
            explosionDamage = damage;
            explosionSize = size;
            travelTime = tTime;
            start = transform.position;
            StartCoroutine(Curve());
        }

        IEnumerator Curve()
        {
            float time = 0f;
 
            Vector3 end = targetPos - (transform.forward * 0.55f); // lead the target a bit to account for travel time, your math will vary

            while(time < travelTime)
            {
                time += Time.deltaTime;
 
                float linearT = time / travelTime;
                float heightT = curve.Evaluate(linearT);
 
                float height = Mathf.Lerp(0f, 5.0f, heightT); // change 3 to however tall you want the arc to be
 
                transform.position = Vector3.Lerp(start, end, linearT) + new Vector3(0f, height,0f);
 
                yield return null;
            }
        }


        protected override void OnHitResponse(HitData data) {
           
           //Explode();
        }
        

        protected override void OnHitObject(Collider other) {
            Explode();
        }

        protected override void OnBulletRecycled() {
            StopAllCoroutines();
        }

        void Explode() {
            SafeGameObjectPool pool = GameObjectPoolManager.Singleton.CreatePool(explosion, 10, 100);

            GameObject exp = pool.Allocate();
            
            //Instantiate(explosion,transform.position,Quaternion.identity);
            exp.transform.position = transform.position;
            exp.transform.rotation = Quaternion.identity;
            Debug.Log("IExplosionViewController: " + exp.GetComponent<IExplosionViewController>());
            exp.GetComponent<IExplosionViewController>().Init(Faction.Neutral, explosionDamage, explosionSize,bulletOwner,
                bulletOwner.GetComponent<ICanDealDamage>());
        }
    }
}