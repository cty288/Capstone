using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using _02._Scripts.Runtime.Utilities;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Spawning;
using System.Threading.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using FIMSpace.FSpine;
using MikroFramework;
using MoreMountains.Feedbacks;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormBossAim : EnemyAction
    {
        private float liftHeight = 20f;
        private float moveRange = 18f;
        public SharedFloat liftTime;
        //making this public for tuning
        public float duration = 5f;
        private float maxTurnRate = 30f;
        private float maxShiftRate = 10f;// Units per second
        private Vector3 gravity = new Vector3(0, -8, 0);
        private int neckJointIndex = 0;
        private int bodyJointIndex = 16;

        public SharedGameObject neckSupportSphere;
        public SharedGameObject bodySupportPlatform;

        private Transform supportSphere;
        private Transform supportPlatform;
        
        private FSpineAnimator.SpineBone _neckJoint;
        private FSpineAnimator.SpineBone _bodyJoint;
        private FSpineAnimator _spine;
        private Transform _lookAt;
        public SharedVector3 originPos;
        private Vector3 _liftedPos;
        private float _progress = 0f;
        private bool _headLifted;
        private bool _running = true;
        public SharedGameObject firePoint;
        
        private NavMeshAgent agent;
        // private Rigidbody rb;
        
        public override void OnAwake()
        {
            agent = gameObject.GetComponent<NavMeshAgent>();
            _spine = GetComponent<FSpineAnimator>();
            _spine.GravityPower = Vector3.zero;
            
            _bodyJoint = _spine.SpineBones[bodyJointIndex];
            _neckJoint = _spine.SpineBones[neckJointIndex];
            Debug.Log($"WORM BOSS: awake {_bodyJoint} at {bodyJointIndex}");
            
            supportSphere = neckSupportSphere.Value.transform;
            supportPlatform = bodySupportPlatform.Value.transform;
            supportSphere.parent = null;
            supportPlatform.parent = null;
            supportSphere.position = Vector3.down * 10000;
            supportPlatform.position = Vector3.down * 10000;
            
            _spine.IncludedColliders.Clear();
            _spine.IncludedColliders.Add(supportSphere.GetComponent<Collider>());
            _spine.IncludedColliders.Add(supportPlatform.GetComponent<Collider>());
        }

        // Start is called before the first frame update
        public override void OnStart()
        {
            base.OnStart();
            
            agent.enabled = false;
            
            supportSphere.parent = null;
            supportPlatform.parent = null;
            supportSphere.gameObject.SetActive(true);
            supportPlatform.gameObject.SetActive(true);
            supportSphere.rotation = Quaternion.identity;
            supportPlatform.rotation = Quaternion.identity;
            
            _lookAt = GetPlayer().transform;
            _headLifted = false;
            _t = 0;
            _progress = 0;
            originPos.SetValue(transform.position);
            _liftedPos = transform.position + Vector3.up * liftHeight;
            _spine.GravityPower = gravity;

            supportPlatform.position = _bodyJoint.FinalPosition + Vector3.up * -2f;
            supportSphere.position = _neckJoint.FinalPosition + Vector3.down * 8f;
            //StartCoroutine(LiftHead());
        }

        // Update is called once per frame
        public override TaskStatus OnUpdate()
        {
            if (!_running) return TaskStatus.Running;
            
            supportSphere.position = _neckJoint.FinalPosition + Vector3.down * 8f;
            supportPlatform.position = new Vector3(_bodyJoint.FinalPosition.x, supportPlatform.position.y, _bodyJoint.FinalPosition.z);
            
            
            // Aim at player
            Aim();
            
            if (!_headLifted)
            {
                // See LiftHead.
                LiftHead();
                return TaskStatus.Running;
            }
            
            if (_progress >= 1f)
            {
                return TaskStatus.Success;
            }
            return TaskStatus.Running;
        }


        private float _t;
        // private bool _lowering;
        private void LiftHead()
        {
            var position = transform.position;
            var headPos = position;
            var originPos = new Vector3(position.x, this.originPos.Value.y, position.z);
            _t += Time.deltaTime/liftTime.Value;
            transform.position = Vector3.Lerp(originPos, headPos, _t);
            if (_t >= 1)
            {
                _headLifted = true;
                _t = 0;
            }
        }

        private void Aim()
        {
            _progress += Time.deltaTime / (duration + liftTime.Value);
            var lookVector = _lookAt.position - firePoint.Value.transform.position;
            var targetRotation = Quaternion.LookRotation(lookVector, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 2f * Time.deltaTime);
            //transform.LookAt(_lookAt.position);

            var moveDistance = moveRange;
            var distancePivot = Vector3.Distance(_lookAt.position, _liftedPos);
            if (distancePivot < moveRange)
            {
                moveDistance = distancePivot - 5f;
            }
            
            if (lookVector.magnitude > 1)
            {
                lookVector = lookVector.normalized;
            }
            transform.position = Vector3.Lerp(transform.position, _liftedPos + lookVector * moveDistance, 2f * Time.deltaTime);
        }
    }
}
