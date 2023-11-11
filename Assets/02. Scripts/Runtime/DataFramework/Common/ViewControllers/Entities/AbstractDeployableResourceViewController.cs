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
		NA
	}
	
	public abstract class AbstractDeployableResourceViewController<T> :
		AbstractResourceViewController<T>, IDeployableResourceViewController where T : class, IResourceEntity, new()  {
		[Header("Deploy Settings")]
		[SerializeField] private float maxSlopeAngle = 45f;
		[field: SerializeField] public bool DoesRotateToSlope { get; protected set; } = true;
		[SerializeField] private bool canDeployInAir = false;
		[SerializeField] private BoxCollider heightDetectionCollider;


		[Header("Deply Status Settings")]
		[SerializeField] private MeshRenderer[] deployStatusRenderers;
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

			foreach (MeshRenderer meshRenderer in deployStatusRenderers) {
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
				
				foreach (MeshRenderer meshRenderer in deployStatusRenderers) {
					meshRenderer.material.color = canDeployColor;
				}
				
				//removeEntityWhenLevelEnd = false;
			}else {
				foreach (var col in selfColliders.Keys) {
					col.isTrigger = selfColliders[col];
				}
				
				foreach (MeshRenderer meshRenderer in deployStatusRenderers) {
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
		

		public virtual bool CheckCanDeploy(Vector3 slopeNormal, Vector3 position, bool isAir, out DeployFailureReason failureReason, out Quaternion spawnedRotation) {
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
			var size = Physics.BoxCastNonAlloc(position, heightDetectionCollider.bounds.extents, normal, results,
				Quaternion.identity, height, obstructionLayer);
			for (int i = 0; i < size; i++) {
				var hit = results[i];
				if (hit.collider != null && !selfColliders.ContainsKey(hit.collider) && !hit.collider.isTrigger) {
					failureReason = DeployFailureReason.Obstructed;
					return false;
				}
			}
			
			spawnedRotation = Quaternion.FromToRotation(Vector3.up, slopeNormal);
			failureReason = DeployFailureReason.NoFailure;
			return true;
		}

		public virtual void OnCanDeployFailureStateChanged(DeployFailureReason lastReason,
			DeployFailureReason currentReason) {
			if (currentReason != DeployFailureReason.NoFailure && (lastReason == DeployFailureReason.NoFailure || lastReason == DeployFailureReason.NA)) {
				foreach (MeshRenderer meshRenderer in deployStatusRenderers) {
					meshRenderer.material.DOColor(cannotDeployColor, "_BaseColor", 0.2f);
				}
			}
			else if (currentReason == DeployFailureReason.NoFailure) { 
				foreach (MeshRenderer meshRenderer in deployStatusRenderers) {
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
			
			foreach (MeshRenderer meshRenderer in deployStatusRenderers) {
				meshRenderer.material.color = Color.white;
			}
		}
	}
}