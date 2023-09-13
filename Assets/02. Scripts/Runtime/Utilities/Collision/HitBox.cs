using UnityEngine;

namespace Runtime.Utilities.Collision
{
    /// <summary>
    /// Checks for collision using BoxCast. 
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class HitBox : MonoBehaviour, IHitDetector
    {
        //Related BoxCollider.
        [SerializeField] private BoxCollider m_collider;
        //LayerMask to check for collision.

        private float thickness = 0.025f;
        //Entity that's called when a collision is detected.
        private IHitResponder m_hitResponder;

        public IHitResponder HitResponder { get => m_hitResponder; set => m_hitResponder = value; }

        /// <summary>
        /// Called every frame to check for BoxCast collision.
        /// Creates a HitData object that is sent to the HitResponder and HurtResponder, invoking their responses.
        /// </summary>
        /// <returns>Returns true if a hit is detected.</returns>
        public void CheckHit(HitDetectorInfo hitDetectorInfo)
        {
            Vector3 scaledSize = new Vector3(
                m_collider.size.x * transform.lossyScale.x,
                m_collider.size.y * transform.lossyScale.y,
                m_collider.size.z * transform.lossyScale.z
            );
            float distance = scaledSize.y - thickness;
            Vector3 direction = transform.up;
            Vector3 center = transform.TransformPoint(m_collider.center);
            Vector3 start = center + direction * (distance * 0.5f);
            Vector3 halfExtents = new Vector3(scaledSize.x, thickness, scaledSize.z) / 2;
            Quaternion orientation = transform.rotation;

            HitData hitData = null;
            IHurtbox hurtbox = null;
            RaycastHit[] hits = Physics.BoxCastAll(start, halfExtents, direction, orientation, distance, hitDetectorInfo.layer);
            foreach (RaycastHit hit in hits)
            {
                hurtbox = hit.collider.GetComponent<IHurtbox>();
                if (hurtbox != null)
                {
                    hitData = new HitData().SetHitBoxData(m_hitResponder, hurtbox, hit, this, center);
                }

                if (hitData.Validate())
                {
                    // Debug.Log("validate");
                    hitData.HitDetector.HitResponder?.HitResponse(hitData);
                    hitData.Hurtbox.HurtResponder?.HurtResponse(hitData);
                }
            }
        }
    }
}

