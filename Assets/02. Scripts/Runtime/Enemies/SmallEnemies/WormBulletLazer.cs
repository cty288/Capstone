using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using System.Collections;
using MikroFramework.Pool;

using MikroFramework.Pool;
using MikroFramework;
namespace a
{
    public class WormBulletLazer : AbstractDotBulletViewController
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
        //public GameObject particlePrefab;
        private GameObject particleInstance;
       // private SafeGameObjectPool pool;
        

        bool pause = false;



        
        private void Start()
        {

            
            transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
           // pool = GameObjectPoolManager.Singleton.CreatePool(particlePrefab, 10, 20);





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


        protected override void Update()
        {

            base.Update();
            timer -= Time.deltaTime;
            if(this.gameObject.transform.localScale.z >= maxRange)
            {
                this.gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, 30f);

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


        public void SetData(GameObject owner, Vector3 dir, GameObject player, float maxRange)
        {
            face = owner;
            this.dir = dir;
            this.player = player;
            this.maxRange = maxRange;
        }

        protected override void OnHitResponse(HitData data)
        {
            Debug.Log("hi");
        }

        protected override void OnHitObject(Collider other)
        {
            Debug.Log(other.gameObject.name);
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
            
            if (collision.gameObject.layer == 8 || collision.gameObject.layer == 11)
            {
                if(collision.gameObject.layer == 11)
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