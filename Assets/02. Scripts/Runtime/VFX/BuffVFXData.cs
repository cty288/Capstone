using System;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using UnityEngine;

namespace _02._Scripts.Runtime.VFX
{
    // The time the buff VFX is played over.
    public enum BuffVFXType
    {
        Continuous,     // This buff is triggered once on register plays infinitely until it is removed.
        Instantaneous,  // This buff is triggered in one instant and plays only once for that instant.
        Transformative  // This buff is triggered once on register but is given a finite duration over which the vfx may change.
    }

    // Context the buff VFX is played in.
    public enum BuffVFXSpace
    {
        Global3D,   // Buff uses a global system that plays an instance everytime it is run (VFX Graph)
        Local3D,    // Buff is parented to the transform it is attached to (Particle System)
        Screen      // Buff plays on player's screen/UI
    }

    public interface IBuffableVFXViewController
    {
        public Transform[] VFXFramer { get; } // List of transforms to apply the VFx on to cover the entity.
    }
    
    public abstract class AbstractBuffableVFX<T> where T : class, IBuff
    {
        public static Vector3 CorrectiveBaseScale = new Vector3(1, 1, 1); // This is used as the default value over which our buffs are scaled.

        public  abstract BuffVFXSpace VFXSpace { get; }
        public abstract BuffVFXType VFXType { get; }

        public abstract string resourceName { get; }

        private SafeGameObjectPool _vfxPool;
        private GameObject[] _vfxObjects;

        private Func<Transform[]> _vfxFramer;

        public AbstractBuffableVFX(Func<Transform[]> framer)
        {
            _vfxFramer = framer;
        }
        
        // Check the buff played is the correct buff for this event.
        public virtual bool ValidateBuff(IBuff buff)
        {
            if (buff is T)
            {
                return true;
            }

            return false;
        }

        public virtual void PoolBuff()
        {
            _vfxPool = GameObjectPoolManager.Singleton.CreatePoolFromAB(resourceName, null, 3, 10, out GameObject prefab0);

        }

        // Allocate buff from pool (implementation differs)
        public virtual void AllocateBuff()
        {
            PoolBuff();
            var framer = _vfxFramer.Invoke();
            _vfxObjects = new GameObject[framer.Length];

            if (VFXSpace == BuffVFXSpace.Local3D)
            {
                for(int i = 0; i < _vfxObjects.Length; i++)
                {
                    var o = _vfxPool.Allocate();
                    
                    var oTransform = o.transform;
                    oTransform.parent = framer[i];
                    oTransform.localPosition = Vector3.zero;
                    oTransform.localRotation = Quaternion.identity;
                    oTransform.localScale = CorrectiveBaseScale;
                }
            }
        }

        // Trigger the buff.
        public abstract bool TriggerBuff(IBuff buff);

        
        // Recycle the buff (implementation might differ)
        public virtual void RecycleBuff()
        {
            foreach (var o in _vfxObjects)
            {
                o.transform.parent = _vfxPool.transform;
                _vfxPool.Recycle(o);
            }
        }
        
        public void OnBuffUpdate(IBuff buff, BuffUpdateEventType eventType){
            if (!ValidateBuff(buff))
            {
                return;
            }

            if (eventType == BuffUpdateEventType.OnStart)
            {
                AllocateBuff();
                return;
            }

            if (eventType == BuffUpdateEventType.OnUpdate)
            {
                TriggerBuff(buff);
                return;
            }

            if (eventType == BuffUpdateEventType.OnEnd)
            {
                RecycleBuff();
                return;
            }
        }
    }

    // Using EMP for Testing Purposes
    public class GenericBuffableVFX : AbstractBuffableVFX<MalfunctionBuff>
    {
        public override BuffVFXSpace VFXSpace { get; } = BuffVFXSpace.Local3D;
        public override BuffVFXType VFXType { get; } = BuffVFXType.Continuous;
        public override string resourceName { get; } = "HCFX_Stun";


        public GenericBuffableVFX(Func<Transform[]> framer) : base(framer)
        {
        }

        public override bool TriggerBuff(IBuff buff)
        {
            // nothing
            return false;
        }
    }
}