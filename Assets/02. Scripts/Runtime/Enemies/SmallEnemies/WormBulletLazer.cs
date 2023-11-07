using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace a
{
    public class WormBulletLazer : AbstractBulletViewController
    {
        public BoxCollider boxCollider;
        public LineRenderer lineRenderer;
        private float damage;
        private float bulletSpeed = 2f;
        private Vector3 dir;
        private GameObject face;
        
        

        private void Start()
        {
            lineRenderer = this.gameObject.GetComponent<LineRenderer>();
            boxCollider = this.gameObject.GetComponent<BoxCollider>();
            boxCollider.size = new Vector3(0.3f, 0.3f, 3f);
            lineRenderer.SetPosition(0, face.transform.position);
            lineRenderer.SetPosition(1, face.transform.position);


        }


        private void Update()
        {
            
            Vector3 nextPosition = lineRenderer.GetPosition(1) + dir * bulletSpeed * Time.deltaTime;
            lineRenderer.SetPosition(1, nextPosition);
            AdjustBoxCollider();

        }
        void AdjustBoxCollider()
        {
            var end = lineRenderer.GetPosition(1);
            var start = lineRenderer.GetPosition(0);
            boxCollider.center = start + (end - start) / 2;
            boxCollider.size = new Vector3(0.3f, 0.3f, end.z);
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

        public void SetData(GameObject owner , Vector3 dir)
        {
            face = owner;
            this.dir = dir;
        }

        protected override void OnHitResponse(HitData data)
        {

        }

        protected override void OnHitObject(Collider other)
        {

        }

        protected override void OnBulletRecycled()
        {
            
        }

        protected override void OnBulletReachesMaxRange()
        {

        }
    }
}
