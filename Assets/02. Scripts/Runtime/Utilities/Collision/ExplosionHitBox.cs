using System;
using MikroFramework.Utilities;
using UnityEngine;

namespace Runtime.Utilities.Collision
{
    /// <summary>
    /// Checks for collision using BoxCast. 
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(TriggerCheck))]
    public class ExplosionHitBox : HitBox
    {
        private SphereCollider _collider;
        private TriggerCheck _triggerCheck;
        private IHitResponder m_hitResponder;
        public override IHitResponder HitResponder { get => m_hitResponder; set => m_hitResponder = value; }
        
        [SerializeField] private bool showDamageNumber = true;
        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            _triggerCheck = gameObject.GetComponent<TriggerCheck>();
            if (_triggerCheck.TargetLayers ==0)
                _triggerCheck.TargetLayers = LayerMask.GetMask("Hurtbox"); 
            _collider = gameObject.GetComponent<SphereCollider>();
        }
        
        

        
        public override void TriggerCheckHit(Collider c)
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

                float explosionMultiplier = 0.7f* (1-Vector3.Distance(hitPoint,center)/_collider.radius)+0.3f;
                // Debug.Log("make hitdata");
                hitData = new HitData()
                    {
                        Damage = m_hitResponder == null ? 0 : Mathf.FloorToInt(Damage * hurtbox.DamageMultiplier*explosionMultiplier),
                        HitPoint = hitPoint == Vector3.zero ? center : hitPoint,
                        HitNormal = hitNormal,
                        Hurtbox = hurtbox,
                        HitDetector = this,
                        Attacker = m_hitResponder,
                        ShowDamageNumber = showDamageNumber
                    };
                // Debug.Log("validate: " + (hitData.Validate()));
                if (hitData.Validate())
                {
                    // Debug.Log("validate: ");
                    hitData.HitDetector.HitResponder?.HitResponse(hitData);
                    hitData.Hurtbox.HurtResponder?.HurtResponse(hitData);
                }
            }


        }

        private void OnDestroy() {
            StopCheckingHits();
        }
    }
}

