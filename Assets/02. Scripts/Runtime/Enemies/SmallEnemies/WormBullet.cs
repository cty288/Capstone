using System.Collections;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
namespace a
{
    public class WormBullet : AbstractBulletViewController
    {
        private float bulletSpeed;
        private GameObject player; // Reference to the player's transform
        private float rotationSpeed;
        [SerializeField] private float maxRotationAngle = 60f;
        private float timer = 0.5f;
        public float curveHeight = 20f; // Height of the Bezier curve
        private float t = 0f;
        private Vector3 previousPosition;
        private Vector3 target;
        private Vector3 start;
        bool flag = false;
        public GameObject explosion;

        private void Start()
        {
            previousPosition = transform.position;
            target = player.transform.position + Random.onUnitSphere * 5;
            target.y = player.transform.position.y;
            start = this.transform.position;
        }
        private void OnEnable()
        {
            
        }
        protected override void Update()
        {
            if (player != null && flag == false)
            {
                start = this.transform.position;
                previousPosition = transform.position;
                target = player.transform.position + Random.onUnitSphere * 8;
                target.y = player.transform.position.y;
                flag = true;
            }
            // If the bullet hasn't reached the player yet
            if (t < 1f)
            {
                // Move the bullet along the Bezier curve
                Vector3 p0 = start;
                Vector3 p1 = p0 + Vector3.up * curveHeight;
                Vector3 p2 = target + Vector3.up * curveHeight;
                p2.y = p1.y;
                Vector3 p3 = target;
                p3.y = p0.y;

                // Calculate the position on the Bezier curve based on constant speed
                t += Time.deltaTime / Vector3.Distance(p0, p3) * bulletSpeed;

                // Ensure t stays in the [0, 1] range
                t = Mathf.Clamp01(t);

                // Bezier curve equation
                Vector3 position = BezierCurve(t, p0, p1, p2, p3);

                transform.position = position;

                // Calculate the direction vector based on the change in position
                Vector3 direction = (position - previousPosition).normalized;

                // Rotate the bullet to face the direction vector
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }

                // Save the current position for the next frame
                previousPosition = position;

                // Increment the parameter for the next frame
                
            }
            else
            {
                // If the bullet has reached the player, move forward in a straight line
                transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
            }
        }
        private Vector3 BezierCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }
        private Vector3 BezierCurveDerivative(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float ttt = tt * t;
            float uuu = uu * u;

            Vector3 derivative = -3 * uuu * p0;
            derivative += (3 * uu - 6 * t * u) * p1;
            derivative += (6 * t * u - 3 * tt) * p2;
            derivative += 3 * ttt * p3;

            return derivative;
        }

        public void SetData(float bulletSpeed, GameObject player , float rotationSpeed)
        {
            this.bulletSpeed = bulletSpeed;
            this.player = player;
            this.rotationSpeed = rotationSpeed;
        }

        protected override void OnHitResponse(HitData data)
        {
            
        }

        protected override void OnHitObject(Collider other)
        {
            Explode();
        }

        protected override void OnBulletRecycled()
        {
            timer = 0.5f;
            t = 0f;
            flag = false;
        }

        protected override void OnBulletReachesMaxRange()
        {
            
        }

        void Explode()
        {
            SafeGameObjectPool pool = GameObjectPoolManager.Singleton.CreatePool(explosion, 10, 100);

            GameObject exp = pool.Allocate();

            //Instantiate(explosion,transform.position,Quaternion.identity);
            exp.transform.position = transform.position;
            exp.transform.rotation = Quaternion.identity;
            Debug.Log("IExplosionViewController: " + exp.GetComponent<IExplosionViewController>());
            if (!bulletOwner)
            {
                Debug.Log("BulletOwner is null");
            }
            exp.GetComponent<IExplosionViewController>().Init(Faction.Explosion, 10, 5, bulletOwner,
                bulletOwner.GetComponent<ICanDealDamage>());
        }
    }
}
