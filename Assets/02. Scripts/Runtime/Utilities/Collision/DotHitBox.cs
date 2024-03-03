using System;
using MikroFramework.Utilities;
using UnityEngine;

namespace Runtime.Utilities.Collision
{
    /// <summary>
    /// Checks for collision using BoxCast. 
    /// </summary>
    [RequireComponent(typeof(Collider))]
    //[RequireComponent(typeof(TriggerCheck))]
    public class DotHitBox : MonoBehaviour, IHitDetector
    {
        protected Collider _collider;
        private IHitResponder m_hitResponder;
        public LayerMask TargetLayers;
        public float dotTick;
        private float timer;
        private bool canDealDamage = true;
        public virtual IHitResponder HitResponder { get => m_hitResponder; set => m_hitResponder = value; }
        [SerializeField] protected bool showDamageNumber = true;


        private void Start()
        {
            Initialize();
            timer = dotTick;
        }
        private void Update()
        {
            if(canDealDamage)
            {
                ResetTimer();
            }
            timer -= Time.deltaTime;
            if(timer < 0)
            {
                canDealDamage = true;
            }
        }

        private void Initialize()
        {
          
            _collider = gameObject.GetComponent<Collider>();
        }

        public void StartCheckingHits(int damage)
        {
            this.Damage = damage;
        }

        public void StopCheckingHits()
        {
            
        }

        private void OnTriggerStay(Collider other)
        {
            if (canDealDamage)
            {
                if (PhysicsUtility.IsInLayerMask(other.gameObject, TargetLayers))
                {
                    IHurtbox hurtbox;
                    hurtbox = other.gameObject.GetComponent<IHurtbox>();
                    HurtboxModifier mod = other.GetComponent<HurtboxModifier>();
                    if (mod)
                    {
                        if (mod.IgnoreHurtboxCheck)
                        {
                            return;
                        }

                        if (mod.RedirectActivated)
                        {
                            hurtbox = mod.Hurtbox;
                        }
                    }
                    if(other.isTrigger && hurtbox == null)
                    {
                        return;
                    }
                
                    HitData hitData = null;
                    Vector3 center = _collider.transform.position;
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    Vector3 hitNormal = transform.position - hitPoint;

                    if(hurtbox != null)
                    {
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

                        if (hitData.Validate()) {
                            Debug.Log(hitData.Hurtbox.Owner.name);
                            hitData.HitDetector.HitResponder?.HitResponse(hitData);
                            hitData.Hurtbox.HurtResponder?.HurtResponse(hitData);
                        }
                    }
                    else
                    {
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
                }
            }
        }
        private void ResetTimer()
        {
            canDealDamage = false;
            timer = dotTick;
        }

        public int Damage { get; protected set; }

        private void OnDestroy()
        {
            StopCheckingHits();
        }

        public void CheckHit(HitDetectorInfo hitDetectorInfo, int damage, Collider[] ignoredColliders = null)
        {
            throw new NotImplementedException();
        }
    }
}

