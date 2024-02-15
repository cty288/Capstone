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
        private float liftTime = 1f;
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
        private Vector3 _originPos;
        private Vector3 _liftedPos;
        private float _progress = 0f;
        private bool _headLifted;
        public SharedGameObject firePoint;
        
        public override void OnAwake()
        {
            _spine = GetComponent<FSpineAnimator>();
            _spine.GravityPower = Vector3.zero;
            
            _bodyJoint = _spine.SpineBones[bodyJointIndex];
            _neckJoint = _spine.SpineBones[neckJointIndex];
            var spherePool = GameObjectPoolManager.Singleton.CreatePool(neckSupportSphere.Value, 1, 3);
            var platPool = GameObjectPoolManager.Singleton.CreatePool(bodySupportPlatform.Value, 1, 3);
            supportSphere = spherePool.Allocate().transform;
            supportPlatform = platPool.Allocate().transform;
            supportSphere.position = Vector3.down * 10000;
            supportPlatform.position = Vector3.down * 10000;
            
            _spine.IncludedColliders.Add(supportSphere.GetComponent<Collider>());
            _spine.IncludedColliders.Add(supportPlatform.GetComponent<Collider>());
        }

        // Start is called before the first frame update
        public override void OnStart()
        {
            _lookAt = GetPlayer().transform;
            _headLifted = false;
            _lowering = false;
            _t = 0;
            _progress = 0;
            _originPos = transform.position;
            _liftedPos = transform.position + Vector3.up * liftHeight;
            _spine.GravityPower = gravity;
            supportPlatform.position = _bodyJoint.FinalPosition + Vector3.up * 1f;
            supportSphere.position = _neckJoint.FinalPosition + Vector3.down * 8f;
            //StartCoroutine(LiftHead());
        }

        // Update is called once per frame
        public override TaskStatus OnUpdate()
        {
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
                if (!_lowering)
                {
                    _headY = transform.position.y;
                    _lowering = true;
                }
                if (LowerHead())
                {
                    _spine.GravityPower = Vector3.zero;
                    _lowering = false;
                    supportSphere.position = Vector3.down * 10000;
                    supportPlatform.position = Vector3.down * 10000;
                    return TaskStatus.Success;
                }
            }
            return TaskStatus.Running;
        }


        private float _t;
        private bool _lowering;
        private void LiftHead()
        {
            var position = transform.position;
            var headPos = position;
            var originPos = new Vector3(position.x, _originPos.y, position.z);
            _t += Time.deltaTime/liftTime;
            transform.position = Vector3.Lerp(originPos, headPos, _t);
            if (_t >= 1)
            {
                _headLifted = true;
                _t = 0;
            }
        }

        private float _headY;
        private bool LowerHead()
        {
            _t += Time.deltaTime/liftTime * 0.5f;
            var position = transform.position;
            var headPos = new Vector3(position.x, _headY, position.z);
            var originPos = new Vector3(position.x, _originPos.y, position.z);
            position = Vector3.Lerp(headPos,  originPos, _t);
            transform.position = position;
            if (_t >= 1)
            {
                return true;
            }

            return false;
        }

        private void Aim()
        {
            _progress += Time.deltaTime / (duration + liftTime);
            var lookVector = _lookAt.position - firePoint.Value.transform.position;
            var targetRotation = Quaternion.LookRotation(lookVector, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 2f * Time.deltaTime);
            //transform.LookAt(_lookAt.position);
            
            if (lookVector.magnitude > 1)
            {
                lookVector = lookVector.normalized;
            }
            transform.position = Vector3.Lerp(transform.position, _liftedPos + lookVector * moveRange, 2f * Time.deltaTime);
        }
    }
}
