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
    public class DOTHitBox : HitBox
    {
        private IHitResponder m_hitResponder;
        private bool isCheckingHits;
        public float damageTick = 0.5f;
        
        public override void StartCheckingHits(int damage) {
            base.StartCheckingHits(damage);
            isCheckingHits = true;
            _triggerCheck.OnUpdate += TriggerCheckHit;
        }

        public override void StopCheckingHits()
        {
            base.StopCheckingHits();
            isCheckingHits = false;
        }
        
    }
}

