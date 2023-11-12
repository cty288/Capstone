using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using System.Collections;

namespace a
{
    public class WormBulletLazerNew : AbstractDotBulletViewController
    {
        public GameObject parent;
        public LineRenderer lineRenderer;
        private float damage;
        private float bulletSpeed = 34f;
        private Vector3 dir;
        private GameObject face;
        private float tickrate = 0.1f;
        private float tick = 0.1f;
        private float waitBeforeRotate = 1.5f;
        private float timer = 1.5f;
        private GameObject player;

        bool pause = false;




        private void Start()
        {
            
           
           transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            



        }
        private void OnEnable()
        {
            
            
            transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            
        }
        IEnumerator RotateAttack()
        {
            float time = 2f;
            while (time > 0)
            {

                Vector3 playerLocalPosition = transform.InverseTransformPoint(player.transform.position);
                if (playerLocalPosition.x > 0)
                {
                    float rotationAmount = 10.5f * Time.deltaTime;
                    transform.Rotate(0, rotationAmount, 0);
                    face.gameObject.transform.Rotate(0, rotationAmount, 0);
                }
                else
                {
                    float rotationAmount = -10.5f * Time.deltaTime;
                    transform.Rotate(0, rotationAmount, 0);
                    face.gameObject.transform.Rotate(0, rotationAmount, 0);
                }
                time -= Time.deltaTime;
                yield return null;

            }
            yield return null;
        }


        private void Update()
        {
            
            tick -= Time.deltaTime;
            timer -= Time.deltaTime;
            if (tick < 0) { tick = tickrate;
                if(hitData != null)
                {

                    if (hitObjects.Contains(hitData.Hurtbox.Owner)){
                        hitObjects.Remove(hitData.Hurtbox.Owner);
                    }
                }
            }
            if (!pause)
            {

                transform.localScale += new Vector3(0, 0, 0.5f);
            }


            if (timer < 0)
            {
                timer = waitBeforeRotate;
                StartCoroutine(RotateAttack());
            }
            

        }
        void AdjustBoxCollider()
        {
            var end = lineRenderer.GetPosition(1);
            var start = lineRenderer.GetPosition(0);
            //boxCollider.center = Vector3.zero + new Vector3(0, 0, Vector3.Magnitude(end - start) / 2);
            //boxCollider.size = new Vector3(0.3f, 0.3f, Vector3.Distance(end, start));
            /*
            Bounds bounds = new Bounds(lineRenderer.GetPosition(1), Vector3.zero);
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                bounds.Encapsulate(lineRenderer.GetPosition(i));
            }

            // Set the Box Collider size and center based on the Line Renderer bounds.
            boxCollider.size = bounds.size;
            boxCollider.center = bounds.center - transform.position; // Make sure to subtract the parent's position.
        */
        }


        public void SetData(GameObject owner, Vector3 dir, GameObject player)
        {
            face = owner;
            this.dir = dir;
            this.player = player;
        }

        protected override void OnHitResponse(HitData data)
        {
            //Debug.Log("hi");
        }

        protected override void OnHitObject(Collider other)
        {

        }

        protected override void OnBulletRecycled()
        {
            
            timer = 1.5f;
            pause = false;
           

        }

        protected override void OnBulletReachesMaxRange()
        {

        }
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
            Debug.Log(collision.gameObject.layer);
            if (collision.gameObject.layer == 8 || collision.gameObject.layer == 11)
            {
                Debug.Log("bbbbbbbbbb");
                pause = true;
            }
        }
        private void OnTriggerExit(Collider collision)
        {
            if (collision.gameObject.layer == 8 || collision.gameObject.layer == 11)
            {
                pause = false;
            }
        }
    }
}
