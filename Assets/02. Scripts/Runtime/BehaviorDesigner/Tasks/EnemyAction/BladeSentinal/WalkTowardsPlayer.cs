using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using UnityEngine;
using UnityEngine.AI;

public class WalkTowardsPlayer : EnemyAction<BladeSentinelEntity> {
	private NavMeshAgent agent;
    
	[SerializeField]
	private SharedGameObject player;

	private float minDistance;
	private float maxDistance;
	private float minDistanceToPlayer;
	private float moveSpeed;
	private bool startTooClose = false;
	private NavMeshPath path = null;
	private float targetWalkDistance = 0f;
	private Vector3 lastPosition = Vector3.zero;
	private float totalWalkedDistance = 0f;
	private Animator animator;
	//private Rigidbody rb;
	public override void OnAwake() {
		base.OnAwake();
		agent = GetComponent<NavMeshAgent>();
		animator = gameObject.GetComponentInChildren<Animator>();
		//rb = gameObject.GetComponent<Rigidbody>();
	}

	public override void OnStart() {
		base.OnStart();
		agent.enabled = true;
		//rb.isKinematic = true;
		minDistance = enemyEntity.GetCustomDataValue<float>("walk", "minWalkDistance");
		maxDistance = enemyEntity.GetCustomDataValue<float>("walk", "maxWalkDistance");
		minDistanceToPlayer = enemyEntity.GetCustomDataValue<float>("walk", "minDistanceToPlayer");
		moveSpeed = enemyEntity.GetCustomDataValue<float>("walk", "moveSpeed");
		totalWalkedDistance = 0;
		targetWalkDistance = 0;
		float distanceToPlayer = Vector3.Distance(gameObject.transform.position, player.Value.transform.position);
		startTooClose = distanceToPlayer < minDistanceToPlayer;

		if (!startTooClose) {
			//calculate if navmesh is reachable
			path = new NavMeshPath();
			
			if (agent.CalculatePath(player.Value.transform.position, path) &&
			    path.status == NavMeshPathStatus.PathComplete) {
				  //walk only within the range
				  agent.speed = moveSpeed;
				  targetWalkDistance = Random.Range(minDistance, Mathf.Min(maxDistance, distanceToPlayer));

				  
				  lastPosition = gameObject.transform.position;


				  animator.CrossFadeInFixedTime("Walk", 0.2f);
			}
			else {
				startTooClose = true;
			}
		}
	}

	public override TaskStatus OnUpdate() {
		if (startTooClose) {
			return TaskStatus.Failure;
		}
		
		totalWalkedDistance += Vector3.Distance(gameObject.transform.position, lastPosition);
		lastPosition = gameObject.transform.position;

		if (totalWalkedDistance >= targetWalkDistance || Vector3.Distance(gameObject.transform.position, player.Value.transform.position) < minDistanceToPlayer) {
			animator.CrossFadeInFixedTime("Idle", 0.1f);
			return TaskStatus.Success;
		}
		else {
			//face the path
			//rb.isKinematic = false;
			agent.SetDestination(player.Value.transform.position);
			Vector3 direction = agent.steeringTarget - gameObject.transform.position;
			direction.y = 0;
			Quaternion lookRotation = Quaternion.LookRotation(direction);
			gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, lookRotation, Time.deltaTime * 30f);
			
			
			
			return TaskStatus.Running;
		}
	}

	public override void OnEnd() {
		base.OnEnd();
		agent.enabled = false;
		//rb.isKinematic = false;
	}
}