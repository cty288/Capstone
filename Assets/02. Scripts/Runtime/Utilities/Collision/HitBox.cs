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
        protected Collider _collider;
        protected TriggerCheck _triggerCheck;
        private IHitResponder m_hitResponder;

        public virtual IHitResponder HitResponder { get => m_hitResponder; set => m_hitResponder = value; }
        [SerializeField] protected bool showDamageNumber = true;
        
        
        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            _triggerCheck = gameObject.GetComponent<TriggerCheck>();
            if (_triggerCheck.TargetLayers ==0)
                _triggerCheck.TargetLayers = LayerMask.GetMask("Hurtbox"); 
            _collider = gameObject.GetComponent<Collider>();
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
        
        public virtual void TriggerCheckHit(Collider c)
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
           
            
             
            HitData hitData = null;
            
            Vector3 center = _collider.transform.position;
            Vector3 hitPoint = c.ClosestPoint(transform.position);
            Vector3 hitNormal = transform.position - hitPoint;
            
           
            // Debug.Log("hurtbox: " + hurtbox);
            if (hurtbox != null)
            {
                // Debug.Log("make hitdata");
                hitData = new HitData().SetHitBoxData(m_hitResponder, Damage, hurtbox,
                    hitPoint == Vector3.zero ? center : hitPoint, hitNormal,
                    this, showDamageNumber);
                
                    /*{
                        Damage = m_hitResponder == null ? 0 : Mathf.FloorToInt(Damage * hurtbox.DamageMultiplier),
                        HitPoint = hitPoint == Vector3.zero ? center : hitPoint,
                        HitNormal = hitNormal,
                        Hurtbox = hurtbox,
                        HitDetector = this,
                        Attacker = m_hitResponder,
                        ShowDamageNumber = showDamageNumber
                    };*/
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
                hitData = new HitData().SetHitBoxData(m_hitResponder, Damage, false,null,
                    hitPoint == Vector3.zero ? center : hitPoint, hitNormal,
                    this, showDamageNumber);
                
                /*= new HitData()
                {
                    Damage = Damage,
                    HitPoint = hitPoint == Vector3.zero ? center : hitPoint,
                    HitNormal = hitNormal,
                    Hurtbox = null,
                    HitDetector = this,
                    Attacker = m_hitResponder,
                    ShowDamageNumber = showDamageNumber
                };*/
                if (hitData.HitDetector.HitResponder != null) {
                    hitData = hitData.HitDetector.HitResponder.OnModifyHitData(hitData);
                }
                HitResponder?.HitResponse(hitData);
            }
            // Debug.Log("validate: " + (hitData.Validate()));

        }
        
        /// <summary>
        /// Called every frame to check for BoxCast collision.
        /// Creates a HitData object that is sent to the HitResponder and HurtResponder, invoking their responses.
        /// </summary>
        /// <returns>Returns true if a hit is detected.</returns>
        public void CheckHit(HitDetectorInfo hitDetectorInfo = new HitDetectorInfo(), int damage = 0, Collider[] ignoredColliders = null)
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

