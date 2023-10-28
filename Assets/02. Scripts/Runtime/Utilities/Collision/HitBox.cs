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
        
        public int Damage { get; protected set; }
        
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
        
        public virtual void StartCheckingHits(int damage) {
            this.Damage = damage;
            if (_triggerCheck == null)
            {
                Initialize();
            }
            
            _triggerCheck.Clear();

            _triggerCheck.OnEnter += TriggerCheckHit;
        }
        
        public virtual void StopCheckingHits()
        {
            // Debug.Log("stop checking hits");
            if (_triggerCheck != null) 
                _triggerCheck.OnEnter -= TriggerCheckHit;
        }
        
        public virtual void TriggerCheckHit(Collider c)
        {
            IHurtbox hurtbox;
            hurtbox = c.GetComponent<IHurtbox>();

            if (c.isTrigger && hurtbox == null) {
                return;
            }
            
            Debug.Log("trigger hit detected: " + c.name);
            HitData hitData = null;
            
            Vector3 center = _collider.transform.position;
            Vector3 hitPoint = c.ClosestPoint(transform.position);
            Vector3 hitNormal = transform.position - hitPoint;
            
           
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
                        ShowDamageNumber = showDamageNumber
                    };
                if (hitData.Validate())
                {
                    // Debug.Log("validate: ");
                    hitData.HitDetector.HitResponder?.HitResponse(hitData);
                    hitData.Hurtbox.HurtResponder?.HurtResponse(hitData);
                }
            }
            else {
                hitData = new HitData()
                {
                    Damage = Damage,
                    HitPoint = hitPoint == Vector3.zero ? center : hitPoint,
                    HitNormal = hitNormal,
                    Hurtbox = null,
                    HitDetector = this,
                    Attacker = m_hitResponder,
                    ShowDamageNumber = showDamageNumber
                };
                
                HitResponder?.HitResponse(hitData);
            }
            // Debug.Log("validate: " + (hitData.Validate()));

        }
        
        public void CheckHit(HitDetectorInfo hitDetectorInfo = new HitDetectorInfo(), int damage = 0)
        {
            Debug.Log("checkhit() is replaced for testing"); //REMOVE LATER
        }
        
        private void OnDestroy() {
            StopCheckingHits();
        }
    }
}

