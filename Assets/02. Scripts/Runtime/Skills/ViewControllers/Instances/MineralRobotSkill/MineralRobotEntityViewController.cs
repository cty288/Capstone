using System.Collections;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers.Instances.BaseLevel;
using _02._Scripts.Runtime.Skills.Model.Instance.MineralRobotSkill;
using _02._Scripts.Runtime.Skills.Model.Instance.TurretSkill;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Temporary;
using Runtime.UI;
using UnityEngine;
using UnityEngine.AI;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill {
	public struct MineralRobotUIMsg : UIMsg {
		public MineralRobotEntity entity;
	}
	public class MineralRobotEntityViewController  : AbstractBasicEntityViewController<MineralRobotEntity> {
		protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = false;
		protected ICommonEntityModel commonEntityModel;
		protected NavMeshAgent navMeshAgent;
		private Transform followTarget;
		private float timer = 0;
		private ICurrencySystem currencySystem;
		[SerializeField] private GameObject explosionVFX;
		[SerializeField] private Transform explosionPos;
		[SerializeField] private float teleportDistance = 10f;
		protected override void Awake() {
			base.Awake();
			commonEntityModel = this.GetModel<ICommonEntityModel>();
			navMeshAgent = GetComponent<NavMeshAgent>();
			currencySystem = this.GetSystem<ICurrencySystem>();
		}
		
		public MineralRobotEntity OnBuildEntity(int rarity, int limit, float interval, int count) {
			if (commonEntityModel == null) {
				commonEntityModel = this.GetModel<ICommonEntityModel>();
			}
			var builder = commonEntityModel.GetBuilder<MineralRobotEntity>(rarity);
			builder.SetProperty(new PropertyNameInfo("data", "limit"), limit)
				.SetProperty(new PropertyNameInfo("data", "interval"), interval)
				.SetProperty(new PropertyNameInfo("data", "count"), count);


			return builder.Build();
		}

		public void SetFollowTarget(Transform target) {
			followTarget = target;
		}

		protected override void Update() {
			base.Update();
			if(!followTarget || !navMeshAgent.enabled) return;
			navMeshAgent.SetDestination(followTarget.position);
			

			if (BoundEntity.CollectResourceType.Value != MineralBotCollectResource.None) {
				timer += Time.deltaTime;
				if (timer >= BoundEntity.Interval.Value) {
					timer = 0;
					CurrencyType currencyType = BoundEntity.CollectResourceType.Value.ConvertToCurrencyType();
					int count = BoundEntity.ResourceCollectCountPerInterval.Value;
					currencySystem.AddCurrency(currencyType, count);
					
					BoundEntity.CollectedCount.Value += count;
					if (BoundEntity.CollectedCount.Value >= BoundEntity.Limit.Value) {

						Instantiate(explosionVFX, explosionPos.position, Quaternion.identity);
						
						commonEntityModel.RemoveEntity(BoundEntity.UUID);
					}
				}
			}
			
			
		}

		protected override void FixedUpdate() {
			base.FixedUpdate();
			if (Vector3.Distance(transform.position, followTarget.position) > teleportDistance) {
				Debug.Log("Teleport! Distance: " + Vector3.Distance(transform.position, followTarget.position) + "" +
				          "FollowTarget: " + followTarget.position + "MyPos: " + transform.position);

				if (NavMesh.SamplePosition(followTarget.position + Random.insideUnitSphere * 5
					    , out NavMeshHit hit, 10, NavMesh.AllAreas)) {
					StartCoroutine(ForceResetPosition(hit.position));
				}
				
			}
		}

		public BoxCollider GetSpawnSizeCollider() {
			return transform.Find("SelfSizeCollider").GetComponent<BoxCollider>();
		}
		
		protected override IEntity OnBuildNewEntity() {
			return OnBuildEntity(1, 50, 5, 1);
		}

		protected override void OnEntityStart() {
			navMeshAgent.enabled = false;
			Vector3 targetPos = transform.position + transform.up * GetSpawnSizeCollider().size.y;
			transform.DOMove(targetPos, 1f).OnComplete(() => {
				navMeshAgent.enabled = true;
			});
			if (!followTarget) {
				followTarget = PlayerController.GetClosestPlayer(transform.position).transform;
			}
		}

		protected override void OnPlayerPressInteract() {
			base.OnPlayerPressInteract();
			
			if(UIManager.Singleton.GetPanel<MineralBotUIViewController>(true) != null) {
				return;
			}

			MainUI.Singleton.OpenOrGetClose<MineralBotUIViewController>(null, new MineralRobotUIMsg() {
				entity = BoundEntity
			}, false);
		}

		public override void OnRecycled() {
			base.OnRecycled();
			navMeshAgent.enabled = false;
			followTarget = null;
			isResettingPos = false;
		}

		protected override void OnBindEntityProperty() {
			
		}

		protected override void OnLevelChange(ILevelEntity oldLevel, ILevelEntity newLevel) {
			base.OnLevelChange(oldLevel, newLevel);
			if (newLevel is BaseLevelEntity) {
				commonEntityModel.RemoveEntity(BoundEntity.UUID);
			}
			else {
				navMeshAgent.enabled = false;
				StartCoroutine(ForceResetPosition(followTarget.position + Random.insideUnitSphere * 10));
			}
			
		}
		
		private bool isResettingPos = false;
		private IEnumerator ForceResetPosition(Vector3 targetPos){
			if (isResettingPos) {
				yield break;
			}
			isResettingPos = true;
			navMeshAgent.enabled = false;
			yield return null;
			transform.position = targetPos;
			yield return null;
			navMeshAgent.enabled = true;
			isResettingPos = false;
		}
	}
}