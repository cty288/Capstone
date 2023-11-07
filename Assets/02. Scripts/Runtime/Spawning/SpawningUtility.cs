using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Utilities;
using JetBrains.Annotations;
using MikroFramework;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Spawning.ViewControllers.Instances;
using Runtime.Temporary;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace Runtime.Spawning {
	public static class SpawningUtility {
		private static Collider[] results = new Collider[10];

		private static Vector3 GetNormalAtPoint(Vector3 point) {
			Vector3 normal = Vector3.zero;
			RaycastHit hit;
			if (Physics.Raycast(point, Vector3.down, out hit, 10.0f)) {
				normal = hit.normal;
			}
			return normal;
		}
		
		private static bool IsSlopeTooSteepAtPoint(Vector3 point, float maxSlopeAngle, out Quaternion rotationWithSlope) {
			Vector3 normal = GetNormalAtPoint(point);
			float angle = Vector3.Angle(normal, Vector3.up);
			rotationWithSlope = Quaternion.FromToRotation(Vector3.up, normal);
			return angle > maxSlopeAngle;
		}
		
		
		public static Vector3 FindNavMeshSuitablePosition(
			Func<BoxCollider> spawnSizeGetter, 
			Vector3 desiredPosition,
			int maxAngle,
			int areaMask, 
			[CanBeNull]
			Vector3[] insideArenaRefPoints, 
			float initialSearchRadius, 
			float increment,
			int maxAttempts, 
			out int usedAttempts,
			out Quaternion rotationWithSlope
			) {
			
			if (insideArenaRefPoints == null) {
				insideArenaRefPoints =
					GameObject.FindGameObjectsWithTag("ArenaRefPoint").Select(x => x.transform.position).ToArray();
			}
			
			LayerMask obstructionLayer = LayerMask.GetMask("Default", "Wall");
			
			usedAttempts = 0;
			float currentSearchRadius = initialSearchRadius;
			NavMeshHit navHit;

			//ICreatureViewController creatureViewController = prefab.GetComponent<ICreatureViewController>();
			BoxCollider boxCollider = spawnSizeGetter();
			Vector3 prefabSize = boxCollider.size;


			if (insideArenaRefPoints != null && insideArenaRefPoints.Length > 0) {
				bool satisfied = false;
				foreach (Vector3 point in insideArenaRefPoints) {
					if (!NavMesh.SamplePosition(point, out navHit, 20.0f, areaMask)) {
						continue;
					}

					NavMeshPath insideArenaDetectPath = new NavMeshPath();
			
					NavMesh.CalculatePath(desiredPosition, navHit.position, areaMask, insideArenaDetectPath);
					if (insideArenaDetectPath.status == NavMeshPathStatus.PathComplete) {
						satisfied = true;
						break;
					}
				}
				
				if(!satisfied) {
					usedAttempts++;
					rotationWithSlope = Quaternion.identity;
					return Vector3.negativeInfinity;
				}
			}
			
		
			if (NavMesh.SamplePosition(desiredPosition, out navHit, 1.0f, areaMask)) {
				if(!IsSlopeTooSteepAtPoint(navHit.position, maxAngle, out rotationWithSlope)) {
					var size = Physics.OverlapBoxNonAlloc(navHit.position + new Vector3(0, prefabSize.y / 2, 0), prefabSize / 2, results, Quaternion.identity,
						obstructionLayer);
					if (size == 0) {
						return navHit.position; 
					}

					if (CheckColliders(size)) {
						return navHit.position;
					}
				}
			}
			
			
			while (usedAttempts < maxAttempts)
			{
				Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * currentSearchRadius;
				usedAttempts++;
				randomDirection.y = 0;
				randomDirection = randomDirection.normalized * currentSearchRadius;
				randomDirection += desiredPosition;

				if (NavMesh.SamplePosition(randomDirection, out navHit, currentSearchRadius, areaMask)) {
					if(IsSlopeTooSteepAtPoint(navHit.position, maxAngle, out rotationWithSlope)) {
						currentSearchRadius += increment;
						continue;
					}
					
					//calculate if this point is naviable to the player
					NavMeshPath path = new NavMeshPath();
					bool result = NavMesh.CalculatePath(desiredPosition, navHit.position, areaMask, path);
					if (path.status == NavMeshPathStatus.PathComplete) {
						var size = Physics.OverlapBoxNonAlloc(navHit.position + new Vector3(0, prefabSize.y / 2, 0), prefabSize / 2, results, Quaternion.identity,
							obstructionLayer);

						if (size == 0) {
							return navHit.position; 
						}
					
						if (CheckColliders(size)) {
							return navHit.position;
						}
					}
					// else {
					// 	Debug.Log("Spawn failed: path not found. Attempt number: " + usedAttempts);
					// }
				}
				
				currentSearchRadius += increment;
				
			}
			
			rotationWithSlope = Quaternion.identity;
			return Vector3.negativeInfinity; 
		}

		public static List<GameObject> SpawnBossPillars(int targetNumber, string prefabName, Bounds bounds) {
			var pillarPool =
				GameObjectPoolManager.Singleton.CreatePoolFromAB(prefabName, null, 4, 10, out GameObject prefab);
			BoxCollider pillarSpawnSizeGetter() => prefab.GetComponent<IBossPillarViewController>().SpawnSizeCollider;
			
			List<GameObject> pillars = new List<GameObject>();

			int areaMask = NavMeshHelper.GetSpawnableAreaMask();
			
			var insideArenaCheckPoints =
				GameObject.FindGameObjectsWithTag("ArenaRefPoint").Select(x => x.transform.position).ToArray();

			//create a new bounds, 20% smaller than the original
			bounds = new Bounds(bounds.center, bounds.size * 0.8f);
			float minDistance = bounds.size.x * 0.3f;
			
			for (int i = 0; i < targetNumber; i++) {
				int remainingRetry = 10000;

				while (remainingRetry > 0) {
					remainingRetry--;
					//get a random point in the bounds
					Vector3 randomPoint = new Vector3(
						UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
						UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
						UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
					);
				
					//sample the point on the navmesh
					NavMeshHit navHit;
					if (!NavMesh.SamplePosition(randomPoint, out navHit, 250.0f, areaMask)) {
						continue;
					}

					Vector3 pos = FindNavMeshSuitablePosition(pillarSpawnSizeGetter, navHit.position, 30, areaMask,
						insideArenaCheckPoints, 10, 10, remainingRetry, out int usedAttempts,
						out Quaternion rotationWithSlope);
					
					//rotate y axis randomly
					rotationWithSlope *= Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

					if (float.IsInfinity(pos.magnitude)) {
						remainingRetry -= usedAttempts;
						continue;
					}
					
					//if the distance to any other pillar is too small, retry
					bool tooClose = false;
					foreach (var pillar in pillars) {
						if (Vector3.Distance(pillar.transform.position, pos) < minDistance) {
							//remainingRetry--;
							tooClose = true;
							break;
						}
					}
					if (tooClose) {
						continue;
					}
					
					
					//ok to spawn
					GameObject pillarInstance = pillarPool.Allocate();
					pillarInstance.transform.position = pos;
					pillarInstance.transform.rotation = rotationWithSlope;
					pillars.Add(pillarInstance);
					break;
				}
				
				
			}
			
			return pillars;
		}


		private static bool CheckColliders(int size) {
			for (int i = 0; i < size; i++) {
				var hit = results[i];
				if (hit && !hit.isTrigger) {
					return false;
				}
			}

			return true;
		}
	}
}