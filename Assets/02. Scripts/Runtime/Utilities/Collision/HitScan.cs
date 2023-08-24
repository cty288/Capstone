using System.Collections;
using Runtime.Weapons;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace Runtime.Utilities.Collision
{
    /// <summary>
    /// HitScan checks for collision using Raycast.
    /// </summary>
    public class HitScan : IHitDetector
    {
        private IHitResponder hitResponder;
        public IHitResponder HitResponder { get => hitResponder; set => hitResponder = value; }

        public LineRenderer lr;

        private Vector3 offset = Vector3.zero;

        public HitScan(IHitResponder hitResponder)
        {
            this.hitResponder = hitResponder;
            lr = new LineRenderer();
        }
        
        /// <summary>
        /// Called every frame to check for Raycast collision.
        /// </summary>
        /// <returns>Returns true if hit detected.</returns>
        public bool CheckHit(HitDetectorInfo hitDetectorInfo)
        {
            // Debug.Log("checkhit");
            Transform launchPoint = hitDetectorInfo.launchPoint;
            Camera camera = hitDetectorInfo.camera;
            LayerMask layer = hitDetectorInfo.layer;
            IWeaponEntity weapon = hitDetectorInfo.weapon;
            
            Vector3 origin = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            offset[0] = Random.Range(-weapon.GetSpread().BaseValue, weapon.GetSpread().BaseValue);
            offset[1] = Random.Range(-weapon.GetSpread().BaseValue, weapon.GetSpread().BaseValue);

            HitData hitData = null;
            RaycastHit hit;
            if (Physics.Raycast(origin, camera.transform.forward + offset, out hit, weapon.GetRange().BaseValue, layer))
            {
                // Debug.Log("hit");
                IHurtbox hurtbox = hit.collider.GetComponent<IHurtbox>();
                if (hurtbox != null)
                {
                    hitData = new HitData().SetHitScanData(hitResponder, hurtbox, hit, this);
                }

                if (hitData.Validate())
                {
                    // Debug.Log("validate");
                    hitData.HitDetector.HitResponder?.HitResponse(hitData);
                    hitData.Hurtbox.HurtResponder?.HurtResponse(hitData);
                    
                    Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                    DrawLine(launchPoint.position, ray.GetPoint(weapon.GetRange().BaseValue) + offset);
                    
                    return true;
                }
            }
            
            //TODO: IMPLEMENT LINE RENDERER FOR SPREAD
            // lr.StartCoroutine(Hitscan());
            return false;
        }
        
        private void DrawLine(Vector3 start, Vector3 end)
        {
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
        
        IEnumerator Hitscan()
        {
            lr.enabled = true;
            yield return new WaitForSeconds(0.3f);
            lr.enabled = false;
        }
    }
}

