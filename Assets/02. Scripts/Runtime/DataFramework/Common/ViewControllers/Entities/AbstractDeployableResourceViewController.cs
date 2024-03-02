using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using DG.Tweening;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;


namespace Runtime.DataFramework.ViewControllers.Entities {
	public interface IDeployableResourceViewController : IResourceViewController {
		public bool CheckCanDeploy(Vector3 slopeNormal, Vector3 position, bool isAir, out DeployFailureReason failureReason, out Quaternion spawnedRotation);
		
		public void OnCanDeployFailureStateChanged(DeployFailureReason lastReason, DeployFailureReason currentReason);
		
		public bool DoesRotateToSlope { get; }

		//public IResourceEntity OnBuildEntity(bool isPreview);
		
		public void SetPreview(bool isPreview);

		public void OnDeploy();
		
		public void OnPreviewTerminate();
	}

	public enum DeployFailureReason {
		NoFailure,
		SlopeTooSteep,
		Obstructed,
		InAir,
		BaitInBattle,
		TurretReachMaxCount,
		NA
	}
	
	public abstract class AbstractDeployableResourceViewController<T> :
		AbstractResourceViewController<T>, IDeployableResourceViewController where T : class, IResourceEntity, new()  {
		[Header("Deploy Settings")]
		[SerializeField] private float maxSlopeAngle = 45f;
		[field: SerializeField] public virtual bool DoesRotateToSlope { get; protected set; } = true;
		[SerializeField] private bool canDeployInAir = false;
		[SerializeField] private BoxCollider heightDetectionCollider;


		[Header("Deply Status Settings")]
		[SerializeField] private Renderer[] deployStatusRenderers;
		[SerializeField] private Color canDeployColor = new Color(0f, 1f, 0f, 0.38f);
		[SerializeField] private Color cannotDeployColor = new Color(1f, 0f, 0f, 0.38f);
		

		private RaycastHit[] results = new RaycastHit[10];
		private LayerMask obstructionLayer;
		
		protected bool isPreview = false;
		protected Dictionary<Collider, bool> selfColliders;
		protected ILevelModel levelModel;

		protected override void Awake() {
			base.Awake();
			levelModel = this.GetModel<ILevelModel>();
			obstructionLayer = LayerMask.GetMask("Default", "Ground", "Wall");
			selfColliders = new Dictionary<Collider, bool>();
			
			foreach (var collider in GetComponentsInChildren<Collider>(true)) {
				selfColliders.Add(collider, collider.isTrigger);
			}

			foreach (Renderer meshRenderer in deployStatusRenderers) {
				Material material = Instantiate(meshRenderer.material);
				meshRenderer.material = material;
			}
			
			if(heightDetectionCollider == null) {
				Debug.LogError("No height detection collider found for {gameObject.name}!");
			}
		}
		
		
		public IResourceEntity OnBuildEntity(bool isPreview) {
			SetPreview(isPreview);
			return OnBuildNewEntity(this.isPreview);
		}

		protected override bool CanAutoRemoveEntityWhenLevelEnd => !isPreview;

		public void SetPreview(bool isPreview) {
			//ILevelEntity levelEntity = levelModel.CurrentLevel.Value;
			this.isPreview = isPreview;
			if (isPreview) {
				foreach (var col in selfColliders.Keys) {
					col.isTrigger = true;
				}
				
				foreach (Renderer meshRenderer in deployStatusRenderers) {
					meshRenderer.material.color = canDeployColor;
				}
				
				//removeEntityWhenLevelEnd = false;
			}else {
				foreach (var col in selfColliders.Keys) {
					col.isTrigger = selfColliders[col];
				}
				
				foreach (Renderer meshRenderer in deployStatusRenderers) {
					meshRenderer.material.color = Color.white;
				}
				
				//removeEntityWhenLevelEnd = true;
			}
		}

		public void OnDeploy() {
			SetPreview(false);
			OnDeployed();
		}

		public abstract void OnDeployed();

		public void OnPreviewTerminate() {
			SetPreview(false);
			RecycleToCache();
		}

		protected override IEntity OnBuildNewEntity() {
			return OnBuildEntity(false);
		}

		protected abstract IResourceEntity OnBuildNewEntity(bool isPreview);
		

		public virtual bool CheckCanDeploy(Vector3 slopeNormal, Vector3 position, bool isAir,
			out DeployFailureReason failureReason, out Quaternion spawnedRotation) {
			spawnedRotation = Quaternion.identity;
			if(isAir && !canDeployInAir) {
				failureReason = DeployFailureReason.InAir;
				return false;
			}
			float angle = Vector3.Angle(Vector3.up, slopeNormal);

			
			if (angle > maxSlopeAngle) {
				failureReason = DeployFailureReason.SlopeTooSteep;
				return false;
			}

			float height = heightDetectionCollider.bounds.size.y;
			
			//fit the collider to the slope
			Vector3 normal = DoesRotateToSlope ? slopeNormal : Vector3.up;
		
			//use box cast instead
			//because the object is pivoted at the bottom, we need to move the position up by half the height
			position += normal * height / 2f;
			if(!CheckIsValidSpawnPos(position, heightDetectionCollider.bounds.size)) {
				failureReason = DeployFailureReason.Obstructed;
				return false;
			}

			spawnedRotation = DoesRotateToSlope
				? Quaternion.FromToRotation(Vector3.up, slopeNormal)
				: Quaternion.identity;
				//Quaternion.FromToRotation(Vector3.up, slopeNormal);
			failureReason = DeployFailureReason.NoFailure;
			return true;
		}
		
		private bool CheckIsValidSpawnPos(Vector3 position, Vector3 size) {
			LayerMask obstructionLayer = LayerMask.GetMask("Default", "Ground", "Wall");
			//raycast up, left, right, forward, backward
			Vector3[] directions = {Vector3.up, Vector3.left, Vector3.right, Vector3.forward, Vector3.back};
			//use line cast instead
			
			foreach (var direction in directions) {
				Vector3 start = position + new Vector3(0, size.y / 2, 0);
				Vector3 addedVector = Vector3.zero;
				
				if (direction == Vector3.up) {
					addedVector = new Vector3(0, size.y / 2, 0);
				}
				else if (direction == Vector3.left) {
					addedVector = new Vector3(-size.x / 2, 0, 0);
				}
				else if (direction == Vector3.right) {
					addedVector = new Vector3(size.x / 2, 0, 0);
				}
				else if (direction == Vector3.forward) {
					addedVector = new Vector3(0, 0, size.z / 2);
				}
				else if (direction == Vector3.back) {
					addedVector = new Vector3(0, 0, -size.z / 2);
				}
				
				Vector3 end = start + addedVector;
				if (Physics.Linecast(start, end, out RaycastHit hit, obstructionLayer, QueryTriggerInteraction.Ignore)) {
					if (hit.collider.gameObject == gameObject) {
						continue;
					}
					Vector3 normal = hit.normal;
					float angle = Vector3.Angle(normal, Vector3.up);
					if (angle >= 80) {
						return false;
					}
				}
			}

			return true;
		}

		public virtual void OnCanDeployFailureStateChanged(DeployFailureReason lastReason,
			DeployFailureReason currentReason) {
			if (currentReason != DeployFailureReason.NoFailure && (lastReason == DeployFailureReason.NoFailure || lastReason == DeployFailureReason.NA)) {
				foreach (Renderer meshRenderer in deployStatusRenderers) {
					meshRenderer.material.DOColor(cannotDeployColor, "_BaseColor", 0.2f);
				}
			}
			else if (currentReason == DeployFailureReason.NoFailure) { 
				foreach (Renderer meshRenderer in deployStatusRenderers) {
					meshRenderer.material.DOColor(canDeployColor, "_BaseColor", 0.2f);
				}
			}
		}


		public override void OnRecycled() {
			base.OnRecycled();
			SetPreview(false);
			
			foreach (var selfCollider in selfColliders) {
				selfCollider.Key.isTrigger = selfCollider.Value;
			}
			
			foreach (Renderer meshRenderer in deployStatusRenderers) {
				meshRenderer.material.color = Color.white;
			}
		}
	}
}