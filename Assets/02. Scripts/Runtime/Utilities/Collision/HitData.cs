using System.Collections.Generic;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Weapons.Model.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Utilities.Collision
{
    /// <summary>
    /// Stores data about a detected collision.
    /// </summary>
    public class HitData : IPoolable
    {
        public int Damage = 0;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public Vector3 HitDirectionNormalized;
        public IHurtbox Hurtbox;
        public IHitDetector HitDetector;
        public ICanDealDamage Attacker;
        public bool ShowDamageNumber = true;

        /// <summary>
        /// Sets the data of the hit. Used for HitScan.
        /// </summary>
        /// <param name="hitResponder"></param>
        /// <param name="hurtbox"></param>
        /// <param name="hit"></param>
        /// <param name="hitDetector"></param>
        /// <returns>Returns HitData object.</returns>
        public HitData SetHitScanData(IHitResponder hitResponder, IHurtbox hurtbox, RaycastHit hit, IHitDetector hitDetector, bool showDamageNumber)
        {
            Damage = hitResponder == null ? 0 : Mathf.FloorToInt(hitDetector.Damage * hurtbox.DamageMultiplier);
            HitPoint = hit.point;
            HitNormal = hit.normal;
            Hurtbox = hurtbox;
            HitDetector = hitDetector;
            Attacker = hitResponder;
            ShowDamageNumber = showDamageNumber;
            return this;
        }

        /// <summary>
        /// Checks if the HitData created is valid (doesn't hit itself, responders exist, etc.
        /// </summary>
        /// <returns>Returns true if valid HitData.</returns>
        public bool Validate()
        {
            // Debug.Log("hurtbox: " + Hurtbox + 
            //           " hurtboxHurtResponder: " + Hurtbox.HurtResponder +
            //           " hurtResponder.checkhurt: " + Hurtbox.HurtResponder.CheckHurt(this) +
            //           " hitrespond: " + HitDetector.HitResponder +
            //           " hitrespond.checkhit: " + HitDetector.HitResponder.CheckHit(this));
            
            if (Hurtbox != null)
                if (Hurtbox.CheckHit(this))
                    if (Hurtbox.HurtResponder == null || Hurtbox.HurtResponder.CheckHurt(this))
                        if (HitDetector.HitResponder == null || HitDetector.HitResponder.CheckHit(this))
                            if (HitDetector.HitResponder != null && Hurtbox.HurtResponder != null &&
                                HitDetector.HitResponder.IsSameFaction(Hurtbox.HurtResponder)) {
                                return false;
                            }
                            else {
                                return true;
                            }
                            
            return false;
        }

        public void OnRecycled()
        {
            Damage = 0;
            HitPoint = Vector3.zero;
            HitNormal = Vector3.zero;
            HitDirectionNormalized = Vector3.zero;
            Hurtbox = null;
            HitDetector = null;
            Attacker = null;
        }

        public bool IsRecycled { get; set; }
        public void RecycleToCache()
        {
            SafeObjectPool<HitData>.Singleton.Recycle(this);
        }
    }

    /// <summary>
    /// Place one a GameObject if it will hit other objects.
    /// </summary>
    public interface IHitResponder : IBelongToFaction, ICanDealDamage
    {
        //int Damage { get; }
        public bool CheckHit(HitData data);
        public void HitResponse(HitData data);
        
        public HitData OnModifyHitData(HitData data);
    }

    /// <summary>
    /// Struct for storing data for CheckHit() in HitScan.
    /// </summary>
    public struct HitDetectorInfo
    {
        public Camera camera;
        public LayerMask layer;
        public Transform launchPoint;
        public IWeaponEntity weapon;
    }
    
    /// <summary>
    /// Used by classes that check for collision: HitBox, HitScan, etc.
    /// </summary>
    public interface IHitDetector
    {
        public IHitResponder HitResponder { get; set; }
        public void CheckHit(HitDetectorInfo hitDetectorInfo, int damage); //CheckHit only required for HitScan right now.
        
        public int Damage { get; }
    }

    /// <summary>
    /// Place one a GameObject if it will BE hit by other objects.
    /// </summary>
    public interface IHurtResponder : IBelongToFaction
    {
        public bool CheckHurt(HitData data);
        public void HurtResponse(HitData data);
    }
    
    /// <summary>
    /// Hurtbox interface.
    /// </summary>
    public interface IHurtbox
    {
        public bool Active { get; }
        public GameObject Owner { get; } //TODO: change to entity
        public Transform Transform { get; }
        public IHurtResponder HurtResponder { get; set; }
        public float DamageMultiplier { get; set; }
        public bool CheckHit(HitData data);
    }
}