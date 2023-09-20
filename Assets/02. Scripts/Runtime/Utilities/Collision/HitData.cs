using System.Collections.Generic;
using MikroFramework.Pool;
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
        public float Recoil = 0f;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public Vector3 HitDirectionNormalized;
        public IHurtbox Hurtbox;
        public IHitDetector HitDetector;
        public IBelongToFaction Attacker;

        /// <summary>
        /// Sets the data of the hit. Used for HitScan.
        /// </summary>
        /// <param name="hitResponder"></param>
        /// <param name="hurtbox"></param>
        /// <param name="hit"></param>
        /// <param name="hitDetector"></param>
        /// <returns>Returns HitData object.</returns>
        public HitData SetHitScanData(IHitResponder hitResponder, IHurtbox hurtbox, RaycastHit hit, IHitDetector hitDetector)
        {
            Damage = hitResponder == null ? 0 : Mathf.FloorToInt(hitResponder.Damage * hurtbox.DamageMultiplier);
            HitPoint = hit.point;
            HitNormal = hit.normal;
            Hurtbox = hurtbox;
            HitDetector = hitDetector;
            Attacker = hitResponder;
            return this;
        }

        /// <summary>
        /// Checks if the HitData created is valid (doesn't hit itself, responders exist, etc.
        /// </summary>
        /// <returns>Returns true if valid HitData.</returns>
        public bool Validate()
        {
            if (Hurtbox != null)
                if (Hurtbox.CheckHit(this))
                    if (Hurtbox.HurtResponder == null || Hurtbox.HurtResponder.CheckHurt(this))
                        if (HitDetector.HitResponder == null || HitDetector.HitResponder.CheckHit(this))
                            return true;
            return false;
        }

        public void OnRecycled()
        {
            Damage = 0;
            Recoil = 0f;
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
    public interface IHitResponder : IBelongToFaction
    {
        int Damage { get; }
        public bool CheckHit(HitData data);
        public void HitResponse(HitData data);
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
        public void CheckHit(HitDetectorInfo hitDetectorInfo); //CheckHit only required for HitScan right now.
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