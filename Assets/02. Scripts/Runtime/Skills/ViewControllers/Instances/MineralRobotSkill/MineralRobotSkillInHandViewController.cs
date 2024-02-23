using _02._Scripts.Runtime.Skills.Model.Builders;
using _02._Scripts.Runtime.Skills.Model.Instance.MineralRobotSkill;
using _02._Scripts.Runtime.Skills.Model.Instance.TurretSkill;
using _02._Scripts.Runtime.Skills.ViewControllers.Base;
using _02._Scripts.Runtime.Utilities;
using Cysharp.Threading.Tasks;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;
using Runtime.Player.ViewControllers;
using Runtime.Spawning;
using Runtime.Utilities;
using Runtime.Utilities.AnimatorSystem;
using UnityEngine;
using UnityEngine.AI;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill {
	public class MineralRobotSkillInHandViewController : AbstractInHandSkillViewController<Model.Instance.MineralRobotSkill.MineralRobotSkill> {
		private bool usedBefore = false;

		[SerializeField] private GameObject robotPrefab;
		[SerializeField] private float optimalSpawnDistance = 3f;
		protected override void OnBindEntityProperty() {
			
		}

		protected override void OnEntityStart() {
			base.OnEntityStart();
			this.RegisterEvent<OnPlayerAnimationEvent>(OnPlayerAnimationEvent)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		}

		private void OnPlayerAnimationEvent(OnPlayerAnimationEvent e) {
			if (e.AnimationName == "OnRemoteControllerUse") {
				DoUseSkill();
			}
		}

		private async UniTask DoUseSkill() {
			MineralRobotEntity entity = await SpawnTurret();
			UseSkill(()=>{OnUseSuccess(entity);}, () => { OnUseFail(entity); });
		}

		private void OnUseFail(MineralRobotEntity entity) {
			entity?.RecycleToCache();
		}

		private void OnUseSuccess(MineralRobotEntity entity) {
			BoundEntity.OnSpawnRobot(entity);
		}

		private async UniTask<MineralRobotEntity> SpawnTurret() {
			Vector3 spawnPosition = ownerGameObject.transform.position +
			                        ownerGameObject.transform.forward * optimalSpawnDistance;

			NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 5, NavMeshHelper.GetSpawnableAreaMask());
			spawnPosition = hit.position;

			BoxCollider collider = robotPrefab.GetComponent<MineralRobotEntityViewController>().GetSpawnSizeCollider();
			float height = collider.size.y;
			
			NavMeshFindResult result = await SpawningUtility.FindNavMeshSuitablePosition(gameObject,
				() => collider,
				spawnPosition, 180, NavMeshHelper.GetSpawnableAreaMask(), default, 1, 5, 100);


			if (!result.IsSuccess) {
				//spawn behind the player
				spawnPosition = ownerGameObject.transform.position -
				                ownerGameObject.transform.forward * optimalSpawnDistance;
				NavMesh.SamplePosition(spawnPosition, out hit, 5, NavMeshHelper.GetSpawnableAreaMask());
				spawnPosition = hit.position;


				result = await SpawningUtility.FindNavMeshSuitablePosition(gameObject,
					() => collider,
					spawnPosition, 180, NavMeshHelper.GetSpawnableAreaMask(), default, 1, 5, 100);

				if (!result.IsSuccess) {
				
					int maxAttempt = 500;
					while (maxAttempt > 0) {
						//pick random direction + position
						Vector3 randomDirection = Random.insideUnitSphere;
						randomDirection.y = 0;
						randomDirection.Normalize();
						spawnPosition = ownerGameObject.transform.position +
						                randomDirection * Random.Range(0.5f, optimalSpawnDistance);
						
						NavMesh.SamplePosition(spawnPosition, out hit, 5, NavMeshHelper.GetSpawnableAreaMask());
						spawnPosition = hit.position;
						
						result = await SpawningUtility.FindNavMeshSuitablePosition(gameObject,
							() => collider,
							spawnPosition, 180, NavMeshHelper.GetSpawnableAreaMask(), default, 1, 5, 100);
						if (result.IsSuccess) {
							break;
						}

						maxAttempt -= result.UsedAttempts;
					}
					
					if (!result.IsSuccess) {
						return null;
					}
				}
			}

			var turret = Instantiate(robotPrefab, result.TargetPosition, result.RotationWithSlope);
			turret.transform.position -= turret.transform.up * height;
			
			MineralRobotEntity entity = turret.GetComponent<MineralRobotEntityViewController>().OnBuildEntity(
				BoundEntity.GetLevel(),
				BoundEntity.GetCustomPropertyOfCurrentLevel<int>("limit"),
				BoundEntity.GetCustomPropertyOfCurrentLevel<float>("interval"),
				BoundEntity.GetCustomPropertyOfCurrentLevel<int>("count"));
		
			MineralRobotEntityViewController viewController = turret.GetComponent<MineralRobotEntityViewController>();
			viewController.InitWithID(entity.UUID);
			viewController.SetFollowTarget(ownerGameObject.transform);
			//face the player, but keep x and z rotation 0
			viewController.transform.LookAt(ownerGameObject.transform);
			viewController.transform.localEulerAngles = new Vector3(0, viewController.transform.localEulerAngles.y, 0);

			return entity;
		}

		public override void OnItemStartUse() {
			
		}

		public override void OnItemStopUse() {
			if (usedBefore) {
				return;
			}
			usedBefore = true;
			this.SendCommand<PlayerAnimationCommand>
				(PlayerAnimationCommand.Allocate("ItemUse", AnimationEventType.Trigger, 0));

		}

		public override void OnItemUse() {
			
		}

		protected override IResourceEntity OnInitSkillEntity(SkillBuilder<Model.Instance.MineralRobotSkill.MineralRobotSkill> builder) {
			return builder.FromConfig().Build();
		}

		public override void OnRecycled() {
			base.OnRecycled();
			usedBefore = false;
			this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Shoot", AnimationEventType.ResetTrigger, 0));
		}
	}
}