using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using  Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.Enemies.ViewControllers.Instances.Berserker;
using UnityEngine;
using UnityEngine.AI;

namespace _02._Scripts.Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class BerserkerStomp: EnemyAction<BerserkerEntity>
    {
        private enum Phase
        {
            Ready,
            Stomp,
            Complete
        }

        private Phase _phase;
        
        private TaskStatus taskStatus;
        
        private float _progress;
        
        private float speed;

        private Vector3 _startPos;
        private Vector3 _targetPos;

        private Transform _player;

        private LayerMask mask = LayerMask.GetMask("Default", "Ground", "Wall");
        
        // **** Values to add to spreadsheet.
        private float _stompHeight = 50f;
        private float _stompTime = 1f;
        private float _readySpeed = 50f;
        
        public override void OnStart()
        {
            base.OnStart();
            _player = GetPlayer().transform;

            _phase = Phase.Ready;
            
            speed = enemyEntity.GetCustomDataValue<float>("entity", "speed");

            _startPos = transform.position;
            
            _progress = 0;

            taskStatus = TaskStatus.Running;
        }
        
        public override TaskStatus OnUpdate()
        {
            if (_phase == Phase.Ready)
            {
                Ready();
            }
            else if(_phase == Phase.Stomp)
            {
                Stomp();
            }
            else
            {
                Stun();
            }
            return taskStatus;
        }

        // Fly above the player and hover after them for a while. Drop a range indicator.
        public void Ready()
        {
            Vector3 aboveTargetPos = _player.position + (Vector3.up * _stompHeight);
            var progressRate = speed/Vector3.Distance(_startPos, aboveTargetPos);
            _progress += progressRate * Time.deltaTime;
            transform.position = Vector3.Slerp(_startPos, aboveTargetPos, _progress);
            transform.rotation = Quaternion.LookRotation(aboveTargetPos - transform.position);

            if (_progress >= 1)
            {
                _progress = 0;
                _phase = Phase.Stomp;
                _startPos = aboveTargetPos;
                RaycastHit hit;
                if (Physics.Raycast(aboveTargetPos + Vector3.down * 5f, Vector3.down, out hit, _stompHeight * 2, mask, QueryTriggerInteraction.Ignore))
                {
                    _targetPos = hit.point;
                    
                    // Cast indicator
                    
                }
                else
                {
                    taskStatus = TaskStatus.Failure;
                }
            }
        }

        // Crash down onto the range indicator.
        public void Stomp()
        {
            _progress += Time.deltaTime / _stompTime;
            transform.position = Vector3.Lerp(_startPos, _targetPos, _progress);
            //transform.rotation = Quaternion.LookRotation(Vector3.down, Quaternion.Euler(0, _progress * 360 * 10, 0) * Vector3.forward);

            if (_progress >= 1)
            {
                _progress = 0;
                _phase = Phase.Complete;
            }
        }

        public void Stun()
        {
            _progress += Time.deltaTime * 2f;
            transform.rotation = Quaternion.LookRotation(_player.position - transform.position);

            if (_progress >= 1)
            {
                taskStatus = TaskStatus.Success;
            }
        }
        
        public override void OnEnd()
        {
            base.OnEnd();
        }
    }
}