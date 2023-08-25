using System.Collections;
using Runtime.Weapons;
using Runtime.Weapons.Model.Base;
using UnityEditor;
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
        

        private Vector3 offset = Vector3.zero;
        private Transform _launchPoint;
        private Camera _camera;
        private LineRenderer _lineRenderer;
        private LayerMask _layer;
        private IWeaponEntity _weapon;

        public HitScan(IHitResponder hitResponder)
        {
            this.hitResponder = hitResponder;
        }
        
        /// <summary>
        /// Called every frame to check for Raycast collision.
        /// </summary>
        /// <returns>Returns true if hit detected.</returns>
        public bool CheckHit(HitDetectorInfo hitDetectorInfo)
        {
            // Debug.Log("checkhit");
            _launchPoint = hitDetectorInfo.launchPoint;
            _camera = hitDetectorInfo.camera;
            _lineRenderer = hitDetectorInfo.lineRenderer;
            _layer = hitDetectorInfo.layer;
            _weapon = hitDetectorInfo.weapon;
            
            Vector3 origin = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            //TODO: Adjust spread to be less random and less punishing when further away [?].
            offset[0] = Random.Range(-_weapon.GetSpread().BaseValue, _weapon.GetSpread().BaseValue);
            offset[1] = Random.Range(-_weapon.GetSpread().BaseValue, _weapon.GetSpread().BaseValue);
            offset[2] = Random.Range(-_weapon.GetSpread().BaseValue, _weapon.GetSpread().BaseValue);
            
            HitData hitData = null;
            RaycastHit hit;
            
            if (Physics.Raycast(origin, Vector3.Normalize(_camera.transform.forward + offset), out hit, _weapon.GetRange().BaseValue, _layer))
            {
                // Debug.Log("hit");
                IHurtbox hurtbox = hit.collider.GetComponent<IHurtbox>();
                if (hurtbox != null)
                {
                    hitData = new HitData().SetHitScanData(hitResponder, hurtbox, hit, this);
                    hitData.Recoil = _weapon.GetRecoil().BaseValue;
                    hitData.HitDirectionNormalized = Vector3.Normalize(_camera.transform.forward + offset);
                }

                if (hitData.Validate())
                {
                    // Debug.Log("validate");
                    hitData.HitDetector.HitResponder?.HitResponse(hitData);
                    hitData.Hurtbox.HurtResponder?.HurtResponse(hitData);
                    
                    //Draw hitscan line.
                    SetLine(_launchPoint.position, hitData.HitPoint);
                    CoroutineRunner.Singleton.StartCoroutine(DrawHitscan());
                    
                    return true;
                }
            }
            
            SetLine(_launchPoint.position,  Vector3.Normalize(_camera.transform.forward + offset) * _weapon.GetRange().BaseValue);
            CoroutineRunner.Singleton.StartCoroutine(DrawHitscan());
            return false;
        }
        
        private void SetLine(Vector3 start, Vector3 end)
        {
            _lineRenderer.SetPosition(0, start);
            _lineRenderer.SetPosition(1, end);
        }
        
        IEnumerator DrawHitscan()
        {
            _lineRenderer.enabled = true;
            yield return new WaitForSeconds(0.3f);
            _lineRenderer.enabled = false;
        }
    }
}

