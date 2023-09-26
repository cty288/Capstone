using System.Collections;
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

      
        void Start() {
            start = transform.position;
            StartCoroutine(Curve());
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void Init(Transform target,float tTime, Faction faction, int damage, GameObject bulletOwner) {
            Init(faction, damage, bulletOwner);
            targetPos = target.position;
            travelTime = tTime;
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
           
        }

        protected override void OnBulletRecycled() {
            StopAllCoroutines();
        }
    }
}