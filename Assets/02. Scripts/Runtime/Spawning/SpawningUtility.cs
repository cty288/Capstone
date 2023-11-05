using System;
using JetBrains.Annotations;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Temporary;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace Runtime.Spawning {
	public static class SpawningUtility {
		private static Collider[] results = new Collider[10];
		
		
		public static Vector3 FindNavMeshSuitablePosition(Func<BoxCollider> spawnSizeGetter, Vector3 desiredPosition,
			 int areaMask, Vector3[] insideArenaRefPoints, float initialSearchRadius, float increment, int maxAttempts, out int usedAttempts) {
			
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
					return Vector3.negativeInfinity;
				}

			}
			

			
			if (NavMesh.SamplePosition(desiredPosition, out navHit, 1.0f, areaMask)) {
				var size = Physics.OverlapBoxNonAlloc(navHit.position + new Vector3(0, prefabSize.y / 2, 0), prefabSize / 2, results, Quaternion.identity,
					obstructionLayer);
				if (size == 0) {
					return navHit.position; 
				}

				if (CheckColliders(size)) {
					return navHit.position;
				}
			}

			while (usedAttempts < maxAttempts)
			{
				Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * currentSearchRadius;
				randomDirection.y = 0;
				randomDirection = randomDirection.normalized * currentSearchRadius;
				randomDirection += desiredPosition;

				if (NavMesh.SamplePosition(randomDirection, out navHit, currentSearchRadius, areaMask)) {
					
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
			return Vector3.negativeInfinity; 
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