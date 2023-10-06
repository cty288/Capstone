using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using Runtime.Utilities.Collision;
using UnityEngine;
using Runtime.Temporary.Weapon;
using Runtime.Weapons.ViewControllers.Base;
using Cinemachine;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class JumpTorwards : EnemyAction
    {
        public CinemachineDollyCart cart;
        public CinemachineSmoothPath path;
        public GameObject player;
        public Transform head;

        public override void OnStart()
        {
            ConfigPath();
        }

        public override TaskStatus OnUpdate()
        {
            cart.m_Path = GameObject.Find("PathHolder").GetComponent<CinemachineSmoothPath>();
            cart.enabled = true;

            return TaskStatus.Running;
        }
        public void ConfigPath()
        {
            if(cart == null)
            {
                Debug.Log("cart is not found");
            }
            path.m_Waypoints[0].position = head.position;
            var playerPos = player.transform.position;
            path.m_Waypoints[1].position = (path.m_Waypoints[0].position + playerPos) / 2 + new Vector3(0, 8f, 0);
            path.m_Waypoints[2].position = playerPos;
            
            
        }
    }
}

