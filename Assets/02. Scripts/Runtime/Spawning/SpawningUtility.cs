using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MikroFramework;
using Runtime.DataFramework.ViewControllers.Entities;
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
			Vector3[] insideArenaRefPoints, 
			float initialSearchRadius, 
			float increment, int maxAttempts, 
			out int usedAttempts,
			out Quaternion rotationWithSlope) {
			
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
				randomDirection.y = 0;
				randomDirection = randomDirection.normalized * currentSearchRadius;
				randomDirection += desiredPosition;

				if (NavMesh.SamplePosition(randomDirection, out navHit, currentSearchRadius, areaMask)) {
					if(IsSlopeTooSteepAtPoint(navHit.position, maxAngle, out rotationWithSlope)) {
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
				usedAttempts++;
			}
			
			rotationWithSlope = Quaternion.identity;
			return Vector3.negativeInfinity; 
		}

		public static List<GameObject> SpawnBossPillars(int targetNumber, string prefabName) {
			var pillarPool = GameObjectPoolManager.Singleton.CreatePoolFromAB(prefabName, null, 4, 10, out _);
			
			return null;
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