using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MikroFramework.Utilities
{
    public class TriggerCheck : MonoBehaviour
    {
        public LayerMask TargetLayers;


        private SimpleRC enterRC = new SimpleRC();

        [SerializeField] private float maxDistance = 100;
        [SerializeField] private float detectTime = 1f;

        [SerializeField] private HashSet<Collider> colliders = new HashSet<Collider>();
        /// <summary>
        /// Get all 2D colliders that are in the current trigger of this object
        /// </summary>
        public HashSet<Collider> Colliders => colliders;

        /// <summary>
        /// If there are any collider in the trigger of this object
        /// </summary>
        public bool Triggered
        {
            get { return enterRC.RefCount > 0; }
        }
        public bool IsDot { get; set; }
        public float DotTick { get; set; }
        private float timer;
        public Collider excludeCollider;
        //dot thing
        public Action<Collider> OnEnter = (collider) => { };
        public Action<Collider> OnExit = (collider) => { };
        public Action<Collider> OnStay = (collider) => { };

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                
            }
            List<Collider> removedColliders = new List<Collider>();

            foreach (Collider col in colliders)
            {
                if (col && Vector2.Distance(col.gameObject.transform.position, transform.position)
                    >= maxDistance)
                {
                    enterRC.Release();
                    removedColliders.Add(col);
                    OnExit?.Invoke(col);
                }
                else if (!col || !col.gameObject.activeInHierarchy)
                {
                    enterRC.Release();
                    removedColliders.Add(col);
                    OnExit?.Invoke(col);
                }
            }

            foreach (Collider removedCollider in removedColliders)
            {
                colliders.Remove(removedCollider);
            }
            colliders.RemoveWhere((collider2D1 => collider2D1 == null));




        }

        private void OnTriggerStay(Collider other)
        {
            
            if (timer < 0 && IsDot && other.gameObject.name == "Hurtbox")
            {
                //Debug.Log(other.gameObject.transform.parent.name);
                OnStay?.Invoke(other);
                //enterRC.Retain();
                if (PhysicsUtility.IsInLayerMask(other.gameObject, TargetLayers))
                {
                    colliders.Remove(excludeCollider);
                    OnStay?.Invoke(other);
                    //OnEnter?.Invoke(other);

                }
                timer = DotTick;

            }
        }


        private void OnTriggerEnter(Collider other)
        {
            
            if (PhysicsUtility.IsInLayerMask(other.gameObject, TargetLayers))
            {
                // Debug.Log("enter, layer");
                enterRC.Retain();
                colliders.Add(other);
                OnEnter?.Invoke(other);
            }

        }

        private void OnTriggerExit(Collider other)
        {
            if (PhysicsUtility.IsInLayerMask(other.gameObject, TargetLayers))
            {
                // Debug.Log("exit, layer");
                //enterRC.Release();
                if (colliders.Contains(other))
                {
                    enterRC.Release();
                    colliders.Remove(other);
                    OnExit?.Invoke(other);
                }
            }

        }

        public void Clear()
        {
            enterRC = new SimpleRC();
            colliders.Clear();

        }
    }
}
