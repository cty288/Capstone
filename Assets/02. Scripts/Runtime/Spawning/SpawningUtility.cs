using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using _02._Scripts.Runtime.Utilities;
using Cysharp.Threading.Tasks;
using DataStructures.ViliWonka.KDTree;
using Framework;
using JetBrains.Annotations;
using MikroFramework;
using MikroFramework.ResKit;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Spawning.ViewControllers.Instances;
using Runtime.Temporary;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace Runtime.Spawning {
	public struct NavMeshFindResult {
		public bool IsSuccess;
		public Vector3 TargetPosition;
		public int UsedAttempts;
		public Quaternion RotationWithSlope;
		
		public NavMeshFindResult(bool isSuccess, Vector3 targetPosition, int usedAttempts, Quaternion rotationWithSlope) {
			IsSuccess = isSuccess;
			TargetPosition = targetPosition;
			UsedAttempts = usedAttempts;
			RotationWithSlope = rotationWithSlope;
		}
		
		public NavMeshFindResult(bool isSuccess, int usedAttempts, Quaternion rotationWithSlope) {
			IsSuccess = isSuccess;
			TargetPosition = Vector3.negativeInfinity;
			UsedAttempts = usedAttempts;
			RotationWithSlope = rotationWithSlope;
		}
	}
	
	
	public static class SpawningUtility {
		private static Collider[] results = new Collider[10];
		private static KDTree refPointsKDTree = null;

		private static bool GetNormalAtPoint(Vector3 point, int layerMask, out Vector3 normal, out Vector3 hitPos) {
			normal = Vector3.zero;
			hitPos = Vector3.zero;
			RaycastHit hit;
			if (Physics.Raycast(point, Vector3.down, out hit, 10.0f, layerMask)) {
				normal = hit.normal;
				hitPos = hit.point;
				return true;
			}

			return false;
		}
		
		public static bool IsSlopeTooSteepAtPoint(Vector3 point, float maxSlopeAngle, out Quaternion rotationWithSlope, out Vector3 groundPos) {
			LayerMask layerMask = LayerMask.GetMask("Default", "Wall", "Ground");
			rotationWithSlope = Quaternion.identity;
			groundPos = Vector3.zero;
			if (GetNormalAtPoint(point, layerMask, out Vector3 normal, out groundPos)) {
				float angle = Vector3.Angle(normal, Vector3.up);
				rotationWithSlope = Quaternion.FromToRotation(Vector3.up, normal);
				return angle > maxSlopeAngle;
			}
			return false;
		}
		
		

		public static void UpdateRefPointsKDTree() {
			Vector3[] insideArenaRefPoints =
				GameObject.FindGameObjectsWithTag("ArenaRefPoint").Where(x => x.gameObject.activeInHierarchy)
					.Select(x => x.transform.position).ToArray();

			refPointsKDTree = new KDTree(insideArenaRefPoints, 8);
		}


		public static async UniTask<NavMeshFindResult> FindNavMeshSuitablePosition(
			GameObject finder,
			Func<BoxCollider> spawnSizeGetter,
			Vector3 desiredPosition,
			int maxAngle,
			int areaMask,
			[CanBeNull]
			Bounds insideArenaBounds,
			//[CanBeNull] Vector3[] insideArenaRefPoints,
			float initialSearchRadius,
			float increment,
			int maxAttempts,
			int operationPerFrame = 10
		) {
			NavMeshFindResult result = new NavMeshFindResult();

			if(float.IsInfinity(desiredPosition.magnitude)) {
				return new NavMeshFindResult(false, Vector3.negativeInfinity, 1, Quaternion.identity);
			}
			
			if (insideArenaBounds == default) {
				insideArenaBounds = GameObject.FindGameObjectsWithTag("MapExtent")
					.First(o => o.gameObject.activeInHierarchy)
					.GetComponent<Collider>().bounds;
			}

			if (refPointsKDTree == null) {
				UpdateRefPointsKDTree();
			}
			
			
			
			
			LayerMask obstructionLayer = LayerMask.GetMask("Default", "Wall");

			int usedAttempts = 0;
			Quaternion rotationWithSlope = Quaternion.identity;


			float currentSearchRadius = initialSearchRadius;
			NavMeshHit navHit;

			//ICreatureViewController creatureViewController = prefab.GetComponent<ICreatureViewController>();
			BoxCollider boxCollider = spawnSizeGetter();
			Vector3 prefabSize = boxCollider.size;

			
			
			KDQuery query = new KDQuery();
			List<int> resultIndices = new List<int>();
			query.KNearest(refPointsKDTree, desiredPosition, refPointsKDTree.Count, resultIndices);

			int operations = 0;
			
			if (insideArenaBounds != default) {
				bool satisfied = false;
				foreach (int pointIndex in resultIndices) {
					//await UniTask.Yield();
					
					Vector3 point = refPointsKDTree.Points[pointIndex];
					if (!NavMesh.SamplePosition(point, out navHit, 20.0f, areaMask)) {
						continue;
					}
					NavMeshPath insideArenaDetectPath = new NavMeshPath();
					if(float.IsInfinity(navHit.position.magnitude)) {
						continue;
					}
					NavMesh.CalculatePath(desiredPosition, navHit.position, areaMask, insideArenaDetectPath);
					if (insideArenaDetectPath.status == NavMeshPathStatus.PathComplete) {
						satisfied = true;
						break;
					}
				}


				/*if (!satisfied) {
					usedAttempts++;
					rotationWithSlope = Quaternion.identity;
					return new NavMeshFindResult(false, Vector3.negativeInfinity, usedAttempts, rotationWithSlope);
				}*/
				
				if(!insideArenaBounds.Contains(desiredPosition) || !satisfied) {
					usedAttempts++;
					rotationWithSlope = Quaternion.identity;
					return new NavMeshFindResult(false, Vector3.negativeInfinity, usedAttempts, rotationWithSlope);
				}
			}


			Vector3 res = Vector3.negativeInfinity;

			//await UniTask.Yield();
			
			if (NavMesh.SamplePosition(desiredPosition, out navHit, 1.0f, areaMask)) {
				if (!IsSlopeTooSteepAtPoint(navHit.position, maxAngle, out rotationWithSlope, out _)) {
					var size = Physics.OverlapBoxNonAlloc(navHit.position + new Vector3(0, prefabSize.y / 2, 0),
						prefabSize / 2, results, Quaternion.identity,
						obstructionLayer);
					if (size == 0) {
						res = navHit.position;
					}

					if (CheckColliders(size)) {
						res = navHit.position;
					}
				}
			}
			

			if (!float.IsInfinity(res.magnitude)) {
				result.IsSuccess = true;
				result.TargetPosition = res;
				result.UsedAttempts = usedAttempts;
				result.RotationWithSlope = rotationWithSlope;
				return result;
			}


			


			while (usedAttempts < maxAttempts) {
				if(operations % operationPerFrame == 0)
					await UniTask.Yield();

				operations++;
				
				//await UniTask.Yield();
				Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * currentSearchRadius;
				usedAttempts++;
				randomDirection.y = 0;
				randomDirection = randomDirection.normalized * currentSearchRadius;
				randomDirection += desiredPosition;


				if (NavMesh.SamplePosition(randomDirection, out navHit, currentSearchRadius, areaMask)) {
					if (IsSlopeTooSteepAtPoint(navHit.position, maxAngle, out rotationWithSlope, out _)) {
						currentSearchRadius += increment;
						continue;
					}

					//calculate if this point is naviable to the player
					NavMeshPath path = new NavMeshPath();
					NavMesh.CalculatePath(desiredPosition, navHit.position, areaMask, path);
					if (path.status == NavMeshPathStatus.PathComplete) {
						var size = Physics.OverlapBoxNonAlloc(navHit.position + new Vector3(0, prefabSize.y / 2, 0),
							prefabSize / 2, results, Quaternion.identity,
							obstructionLayer);

						if (size == 0) {
							return new NavMeshFindResult(true, navHit.position, usedAttempts, rotationWithSlope);
							//navHit.position;
						}

						if (CheckColliders(size)) {
							return new NavMeshFindResult(true, navHit.position, usedAttempts, rotationWithSlope);
							//navHit.position;
						}
					}
				}

				currentSearchRadius += increment;

			}

			rotationWithSlope = Quaternion.identity;
			return new NavMeshFindResult(false, Vector3.negativeInfinity, usedAttempts, rotationWithSlope);
		}



		//no advanced checks, but may spawn in walls or outside of arena
		public static Vector3 FindNavMeshSuitablePositionFast(
			Vector3 desiredPosition,
			Func<BoxCollider> spawnSizeGetter,
			int areaMask,
			float initialSearchRadius, 
			float increment,
			int maxAttempts, 
			out int usedAttempts,
			out Quaternion rotationWithSlope
		) {
			
			usedAttempts = 0;
			float currentSearchRadius = initialSearchRadius;
			NavMeshHit navHit;
			BoxCollider boxCollider = spawnSizeGetter();
			Vector3 prefabSize = boxCollider.size;
			while (usedAttempts < maxAttempts)
			{
				Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * currentSearchRadius;
				usedAttempts++;
				randomDirection.y = 0;
				randomDirection = randomDirection.normalized * currentSearchRadius;
				randomDirection += desiredPosition;

				if (NavMesh.SamplePosition(randomDirection, out navHit, currentSearchRadius, areaMask)) {
					if(IsSlopeTooSteepAtPoint(navHit.position, 90, out rotationWithSlope, out _)) {
						currentSearchRadius += increment;
						continue;
					}
					LayerMask obstructionLayer = LayerMask.GetMask("Default", "Wall");
					var size = Physics.OverlapBoxNonAlloc(navHit.position + new Vector3(0, prefabSize.y / 2, 0), prefabSize / 2, results, Quaternion.identity,
						obstructionLayer);

					if (size == 0) {
						return navHit.position; 
					}
					
					if (CheckColliders(size)) {
						return navHit.position;
					}
					
				}
				
				currentSearchRadius += increment;
				
			}

			rotationWithSlope = Quaternion.identity;
			return Vector3.negativeInfinity; 
		}

		public static async UniTask<GameObject> SpawnExitDoor(GameObject spawner, string prefabName, Bounds bounds) {
			GameObject prefab = MainGame.Interface.GetUtility<ResLoader>().LoadSync<GameObject>(prefabName);
			
			BoxCollider spawnSizeGetter() => prefab.GetComponent<LevelExitDoorController>().SpawnSizeCollider;
			int areaMask = NavMeshHelper.GetSpawnableAreaMask();
			Bounds insideArenaBounds = default;
			if (insideArenaBounds == default) {
				insideArenaBounds = GameObject.FindGameObjectsWithTag("MapExtent")
					.First(o => o.gameObject.activeInHierarchy)
					.GetComponent<Collider>().bounds;
			}
			
			

			//create a new bounds, 20% smaller than the original
			bounds = new Bounds(bounds.center, bounds.size * 0.8f);
			
			
			GameObject doorInstance = null;

			while (true) {
				await UniTask.Yield();
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

				NavMeshFindResult res = await FindNavMeshSuitablePosition(spawner, spawnSizeGetter,
					navHit.position, 45, areaMask,
					insideArenaBounds, 10, 10, 100, 500);
					
				//rotate y axis randomly
				//res.RotationWithSlope *= Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

				if (!res.IsSuccess) {
					continue;
				}
				
				//ok to spawn
				doorInstance = GameObject.Instantiate(prefab);
				doorInstance.transform.position = res.TargetPosition;
				doorInstance.transform.rotation = res.RotationWithSlope;
				break;
			}

			return doorInstance;
		}
		
		public static async UniTask<List<GameObject>> SpawnBossPillars(GameObject spawner, int targetNumber, string prefabName, Bounds bounds) {
			var pillarPool =
				GameObjectPoolManager.Singleton.CreatePoolFromAB(prefabName, null, 4, 10, out GameObject prefab);
			BoxCollider pillarSpawnSizeGetter() => prefab.GetComponent<IBossPillarViewController>().SpawnSizeCollider;
			
			List<GameObject> pillars = new List<GameObject>();

			int areaMask = NavMeshHelper.GetSpawnableAreaMask();
			
			/*var insideArenaCheckPoints =
				GameObject.FindGameObjectsWithTag("ArenaRefPoint").Select(x => x.transform.position).ToArray();*/

			Bounds insideArenaBounds = default;
			if (insideArenaBounds == default) {
				insideArenaBounds = GameObject.FindGameObjectsWithTag("MapExtent")
					.First(o => o.gameObject.activeInHierarchy)
					.GetComponent<Collider>().bounds;
			}
			
			

			//create a new bounds, 20% smaller than the original
			bounds = new Bounds(bounds.center, bounds.size * 0.8f);
			float minDistance = bounds.size.x * 0.2f;
			
			for (int i = 0; i < targetNumber; i++) {
				int remainingRetry = 1000;

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

					NavMeshFindResult res = await FindNavMeshSuitablePosition(spawner, pillarSpawnSizeGetter,
						navHit.position, 45, areaMask,
						insideArenaBounds, 10, 10, remainingRetry, 500);
					
					//rotate y axis randomly
					res.RotationWithSlope *= Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

					if (!res.IsSuccess) {
						remainingRetry -= res.UsedAttempts;
						continue;
					}
					
					//if the distance to any other pillar is too small, retry
					bool tooClose = false;
					foreach (var pillar in pillars) {
						if (Vector3.Distance(pillar.transform.position, res.TargetPosition) < minDistance) {
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
					pillarInstance.transform.position = res.TargetPosition;
					pillarInstance.transform.rotation = res.RotationWithSlope;
					pillars.Add(pillarInstance);
					break;
				}
				
				
			}
			
			return pillars;
		}


		private static bool CheckColliders(int size) {
			for (int i = 0; i < size; i++) {
				var hit = results[i];
				if (hit && (!hit.isTrigger || hit.gameObject.CompareTag("SpawnSizeCollider"))) {
					return false;
				}
			}

			return true;
		}
	}
}