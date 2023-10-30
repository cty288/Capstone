using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Temporary;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.Spawning {
	public static class SpawningUtility {
		private static Collider[] results = new Collider[10];
		
		
		public static Vector3 FindNavMeshSuitablePosition(GameObject prefab, Vector3 desiredPosition,
			float initialSearchRadius, float increment, int maxAttempts, out int usedAttempts) {
			
			LayerMask obstructionLayer = LayerMask.GetMask("Default", "Wall");
			
			usedAttempts = 0;
			float currentSearchRadius = initialSearchRadius;
			NavMeshHit navHit;

			ICreatureViewController creatureViewController = prefab.GetComponent<ICreatureViewController>();
			BoxCollider boxCollider = creatureViewController.SpawnSizeCollider;
			Vector3 prefabSize = boxCollider.size;

			/*Transform playerTr = PlayerController.GetClosestPlayer(desiredPosition).transform;
			if (!NavMesh.SamplePosition(playerTr.position, out navHit, 20.0f, -1)) {
				usedAttempts++;
				return Vector3.negativeInfinity; 
			}
			
			NavMeshPath playerDetectPath = new NavMeshPath();
			
			NavMesh.CalculatePath(desiredPosition, navHit.position, -1, playerDetectPath);
			if (playerDetectPath.status != NavMeshPathStatus.PathComplete) {
				usedAttempts++;
				return Vector3.negativeInfinity; 
			}*/

			
			if (NavMesh.SamplePosition(desiredPosition, out navHit, 1.0f, -1)) {
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

				if (NavMesh.SamplePosition(randomDirection, out navHit, currentSearchRadius, -1)) {
					
					//calculate if this point is naviable to the player
					NavMeshPath path = new NavMeshPath();
					bool result = NavMesh.CalculatePath(desiredPosition, navHit.position, -1, path);
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