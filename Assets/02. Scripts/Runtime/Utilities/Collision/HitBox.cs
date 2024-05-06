using System;
using MikroFramework.Utilities;
using UnityEngine;

namespace Runtime.Utilities.Collision
{
    /// <summary>
    /// Checks for collision using BoxCast. 
    /// </summary>
    //[RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(TriggerCheck))]
    public class HitBox : MonoBehaviour, IHitDetector {
        [SerializeField] private Collider overrideCollider;
        protected Collider _collider;
        protected TriggerCheck _triggerCheck;
        private IHitResponder m_hitResponder;

        public virtual IHitResponder HitResponder { get => m_hitResponder; set => m_hitResponder = value; }
        [SerializeField] protected bool showDamageNumber = true;
        
        private HitData hitData;
        
        private void Start()
        {
            Initialize();
            hitData = new HitData();
        }

        private void Initialize()
        {
            
            _triggerCheck = gameObject.GetComponent<TriggerCheck>();
            if (_triggerCheck.TargetLayers ==0)
                _triggerCheck.TargetLayers = LayerMask.GetMask("Hurtbox");
            if (!overrideCollider) {
                _collider = gameObject.GetComponent<Collider>();
            }
            else {
                _collider = overrideCollider;
            }
            if (_collider == null) {
                Debug.LogError("HitBox: No collider found on object: " + gameObject.name);
            }
            
        }
        
        public void StartCheckingHits(int damage) {
            this.Damage = damage;
            if (_triggerCheck == null)
            {
                Initialize();
            }
            
            _triggerCheck.Clear();

            _triggerCheck.OnEnter += TriggerCheckHit;
        }
        
        public void StopCheckingHits()
        {
            if (_triggerCheck != null) 
                _triggerCheck.OnEnter -= TriggerCheckHit;
        }
        
        protected virtual void TriggerCheckHit(Collider c)
        {
            IHurtbox hurtbox;
            hurtbox = c.GetComponent<IHurtbox>();

            HurtboxModifier hurtboxModifier = c.GetComponent<HurtboxModifier>();
            if (hurtboxModifier) {
                if (hurtboxModifier.IgnoreHurtboxCheck) {
                    return;
                }
                    
                if (hurtboxModifier.RedirectActivated) {
                    hurtbox = hurtboxModifier.Hurtbox;
                }
            }
            
            if (c.isTrigger && hurtbox == null) {
                return;
            }
             
            Vector3 center = _collider.transform.position;
            Vector3 hitPoint = c.ClosestPoint(transform.position);
            Vector3 hitNormal = transform.position - hitPoint;
            
            if (hurtbox != null)
            {
                hitData.ResetHitData();
                hitData.SetHitBoxData(m_hitResponder, Damage, hurtbox,
                    hitPoint == Vector3.zero ? center : hitPoint, hitNormal,
                    this, showDamageNumber);
                
                if (hitData.Validate())
                {
                    // Debug.Log("validate: ");
                    if (hitData.HitDetector.HitResponder != null) {
                        hitData = hitData.HitDetector.HitResponder.OnModifyHitData(hitData);
                    }
                    hitData.HitDetector.HitResponder?.HitResponse(hitData);
                    hitData.Hurtbox.HurtResponder?.HurtResponse(hitData);
                }
            }
            else {
                hitData.ResetHitData();
                hitData.SetHitBoxData(m_hitResponder, Damage, false,null,
                    hitPoint == Vector3.zero ? center : hitPoint, hitNormal,
                    this, showDamageNumber);
                
                if (hitData.HitDetector.HitResponder != null) {
                    hitData = hitData.HitDetector.HitResponder.OnModifyHitData(hitData);
                }
                HitResponder?.HitResponse(hitData);
            }
        }
        
        /// <summary>
        /// Called every frame to check for BoxCast collision.
        /// Creates a HitData object that is sent to the HitResponder and HurtResponder, invoking their responses.
        /// </summary>
        /// <returns>Returns true if a hit is detected.</returns>
        public void CheckHit(HitDetectorInfo hitDetectorInfo = new HitDetectorInfo(), int damage = 0, Collider[] ignoredColliders = null)
        {
        }

        public int Damage { get; protected set; }

        private void OnDestroy() {
            StopCheckingHits();
        }
    }
}

