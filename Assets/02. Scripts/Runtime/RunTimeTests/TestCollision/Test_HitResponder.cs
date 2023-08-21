using System.Collections.Generic;
using Runtime.Temporary;
using Runtime.Utilities.AnimationEvents;
using Runtime.Utilities.Collision;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.RunTimeTests.TestCollision
{
    public class Test_HitResponder : MonoBehaviour, IHitResponder
    {
        //Enity Stats.
        [Header("Entity Information")]
        public NavMeshAgent agent;
        public TestEnity entity;
        [SerializeField] private int m_damage = 10;

        //Animation-related.
        [Header("Animation")]
        public Animator animator;
        public AnimationSMBManager animationSMBManager;

        //Collision-related.
        [Header("Hitboxes")]
        [SerializeField] private bool punching = false;

        [SerializeField] private HitBox hitbox_RightHand;

        [Header("Hit Effects")]
        [SerializeField] private GameObject hitParticlePrefab;

        private List<GameObject> hitObjects = new List<GameObject>();

        public int Damage => m_damage;

        public void Start()
        {
            entity = gameObject.GetComponent<TestEnity>();

            //Animation-related.
            // animator = GetComponent<Animator>();
            animationSMBManager = GetComponent<AnimationSMBManager>();
            animationSMBManager.Event.AddListener(OnAnimationEvent);

            //Collision-related.
            hitbox_RightHand.HitResponder = this;
        }

        private void Update()
        {
            //Animation-related.
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        private void FixedUpdate()
        {
            //Collision-related.
            if (punching)
            {
                hitbox_RightHand.CheckHit();
            }
        }

        public bool CheckHit(HitData data)
        {
            if (data.Hurtbox.Owner == gameObject) { return false; }
            else if (hitObjects.Contains(data.Hurtbox.Owner)) { return false; }
            else { return true; }
        }

        public void HitResponse(HitData data)
        {
            hitObjects.Add(data.Hurtbox.Owner);

            Instantiate(hitParticlePrefab, data.HitPoint, Quaternion.identity);
        }

        public void OnAnimationEvent(string eventName)
        {
            // Debug.Log("Animation Event: " + eventName);
            switch (eventName)
            {
                case "PunchStart":
                    hitObjects.Clear();
                    punching = true;
                    break;
                case "PunchEnd":
                    punching = false;
                    break;
                default:
                    break;
            }
        }
    }
}
