using System.Collections.Generic;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
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
        private HitDetectorInfo hitDetectorInfo;

        public int Damage => m_damage;
        
        [field: ES3Serializable]
        public BindableProperty<Faction> CurrentFaction { get; protected set; } = new BindableProperty<Faction>(Faction.Hostile);


        public void Start()
        {
            entity = gameObject.GetComponent<TestEnity>();

            //Animation-related.
            // animator = GetComponent<Animator>();
            animationSMBManager = GetComponent<AnimationSMBManager>();
            animationSMBManager.Event.AddListener(OnAnimationEvent);

            //Collision-related.
            hitbox_RightHand.HitResponder = this;
            // hitDetectorInfo = new HitDetectorInfo();
        }

        private void Update()
        {
            //Animation-related.
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        public bool CheckHit(HitData data)
        {
            if (data.Hurtbox.Owner == gameObject) { return false; }
            else if (hitObjects.Contains(data.Hurtbox.Owner)) { return false; }
            else { return true; }
        }

        public void HitResponse(HitData data)
        {
            Debug.Log("hit response, punching: " + punching);
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
                    hitbox_RightHand.StartCheckingHits();
                    break;
                case "PunchEnd":
                    punching = false;
                    hitbox_RightHand.StopCheckingHits();
                    break;
                default:
                    break;
            }
        }
    }
}
