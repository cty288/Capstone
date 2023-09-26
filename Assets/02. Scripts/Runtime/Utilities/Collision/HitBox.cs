using System;
using MikroFramework.Utilities;
using UnityEngine;

namespace Runtime.Utilities.Collision
{
    /// <summary>
    /// Checks for collision using BoxCast. 
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(TriggerCheck))]
    public class HitBox : MonoBehaviour, IHitDetector
    {
        private Collider _collider;
        private TriggerCheck _triggerCheck;
        private IHitResponder m_hitResponder;

        public IHitResponder HitResponder { get => m_hitResponder; set => m_hitResponder = value; }
        
        
        
        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            _triggerCheck = gameObject.GetComponent<TriggerCheck>();
            _triggerCheck.TargetLayers = LayerMask.GetMask("Hurtbox"); 
            _collider = gameObject.GetComponent<Collider>();
        }
        
        public void StartCheckingHits(int damage) {
            this.Damage = damage;
            if (_triggerCheck == null)
            {
                Initialize();
            }

            _triggerCheck.OnEnter += TriggerCheckHit;
        }
        
        public void StopCheckingHits()
        {
            // Debug.Log("stop checking hits");
            _triggerCheck.OnEnter -= TriggerCheckHit;
        }
        
        public void TriggerCheckHit(Collider c)
        {
            // Debug.Log("trigger hit detected: " + c.name);
            HitData hitData = null;
            IHurtbox hurtbox;
            Vector3 center = _collider.transform.position;
            Vector3 hitPoint = c.ClosestPoint(transform.position);
            Vector3 hitNormal = transform.position - hitPoint;
            
            hurtbox = c.GetComponent<IHurtbox>();
            // Debug.Log("hurtbox: " + hurtbox);
            if (hurtbox != null)
            {
                // Debug.Log("make hitdata");
                hitData = new HitData()
                    {
                        Damage = m_hitResponder == null ? 0 : Mathf.FloorToInt(Damage * hurtbox.DamageMultiplier),
                        HitPoint = hitPoint == Vector3.zero ? center : hitPoint,
                        HitNormal = hitNormal,
                        Hurtbox = hurtbox,
                        HitDetector = this,
                        Attacker = m_hitResponder,
                    };
            }

            // Debug.Log("validate: " + (hitData.Validate()));
            if (hitData.Validate())
            {
                // Debug.Log("validate: ");
                hitData.HitDetector.HitResponder?.HitResponse(hitData);
                hitData.Hurtbox.HurtResponder?.HurtResponse(hitData);
            }
        }
        
        /// <summary>
        /// Called every frame to check for BoxCast collision.
        /// Creates a HitData object that is sent to the HitResponder and HurtResponder, invoking their responses.
        /// </summary>
        /// <returns>Returns true if a hit is detected.</returns>
        public void CheckHit(HitDetectorInfo hitDetectorInfo = new HitDetectorInfo(), int damage = 0)
        {
            Debug.Log("checkhit() is replaced for testing");
            
            
            // Vector3 scaledSize = new Vector3(
            //     m_collider.size.x * transform.lossyScale.x,
            //     m_collider.size.y * transform.lossyScale.y,
            //     m_collider.size.z * transform.lossyScale.z
            // );
            // float distance = scaledSize.y - thickness;
            // Vector3 direction = transform.up;
            // Vector3 center = transform.TransformPoint(m_collider.center);
            // Vector3 start = center + direction * (distance * 0.5f);
            // Vector3 halfExtents = new Vector3(scaledSize.x, thickness, scaledSize.z) / 2;
            // Quaternion orientation = transform.rotation;
            //
            // HitData hitData = null;
            // IHurtbox hurtbox = null;
            // RaycastHit[] hits = Physics.BoxCastAll(start, halfExtents, direction, orientation, distance, hitDetectorInfo.layer);
            // foreach (RaycastHit hit in hits)
            // {
            //     hurtbox = hit.collider.GetComponent<IHurtbox>();
            //     if (hurtbox != null)
            //     {
            //         hitData = new HitData().SetHitBoxData(m_hitResponder, hurtbox, hit, this, center);
            //     }
            //
            //     if (hitData.Validate())
            //     {
            //         // Debug.Log("validate");
            //         hitData.HitDetector.HitResponder?.HitResponse(hitData);
            //         hitData.Hurtbox.HurtResponder?.HurtResponse(hitData);
            //     }
            // }
        }

        public int Damage { get; protected set; }

        private void OnDestroy() {
            StopCheckingHits();
        }
    }
}

