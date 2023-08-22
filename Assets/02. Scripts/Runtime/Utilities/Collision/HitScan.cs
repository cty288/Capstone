using UnityEngine;

namespace Runtime.Utilities.Collision
{
    /// <summary>
    /// HitScan checks for collision using Raycast.
    /// </summary>
    public class HitScan : IHitDetector
    {
        // public BasicGun originWeapon; //TODO: change to weapon class
        public Camera cam;
        public float range;
        public LayerMask layer;

        private IHitResponder m_hitResponder;

        public IHitResponder HitResponder { get => m_hitResponder; set => m_hitResponder = value; }

        /// <summary>
        /// Constructor for HitScan.
        /// </summary>
        /// <param name="cam">Needed for calculating origin and ending of Raycast.</param>
        /// <param name="range">Range of Raycast.</param>
        /// <param name="layer"></param>
        /// <param name="weapon"></param>
        public HitScan(Camera cam, float range, LayerMask layer, IHitResponder weapon)
        {
            this.cam = cam;
            this.range = range;
            this.layer = layer;
            m_hitResponder = weapon;
        }

        /// <summary>
        /// Called every frame to check for Raycast collision.
        /// </summary>
        /// <returns>Returns true if hit detected.</returns>
        public bool CheckHit()
        {
            HitData hitData = null;

            Vector3 origin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (Physics.Raycast(origin, cam.transform.forward, out hit, range, layer))
            {
                IHurtbox hurtbox = hit.collider.GetComponent<IHurtbox>();
                if (hurtbox != null)
                {
                    hitData = new HitData().SetHitScanData(m_hitResponder, hurtbox, hit, this);
                }

                if (hitData.Validate())
                {
                    // Debug.Log("validate");
                    hitData.HitDetector.HitResponder?.HitResponse(hitData);
                    hitData.Hurtbox.HurtResponder?.HurtResponse(hitData);
                    return true;
                }
            }
            return false;
        }
    }
}

