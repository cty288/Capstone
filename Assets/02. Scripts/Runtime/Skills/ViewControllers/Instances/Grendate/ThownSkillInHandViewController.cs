using System.Collections.Generic;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Skills.Model.Builders;
using _02._Scripts.Runtime.Skills.Model.Instance;
using _02._Scripts.Runtime.Skills.ViewControllers.Base;
using DG.Tweening;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.GameResources.Model.Base;
using Runtime.Player.ViewControllers;
using Runtime.Utilities;
using Runtime.Utilities.AnimatorSystem;
using Runtime.Weapons.ViewControllers;
using UnityEngine;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances {
	public abstract class ThownSkillInHandViewController<T>  : AbstractInHandSkillViewController<T> where T : class, ISkillEntity, new() {
		[SerializeField]
		private GameObject grenadePrefab;

		[SerializeField]
		private float throwForce = 30f;
		
		[SerializeField]
		private LayerMask obstacleLayerMask;
		[SerializeField]
		private LineRenderer trajectoryLine;

		[SerializeField] 
		private GameObject hitRangePreviewPrefab;

		private GameObject spawnedHitRangePreview;
		private Transform throwPoint;
		private Material trajectoryLineMaterial;
		private Tween materialTween;
		private float materialOriginalAlpha;

		private SafeGameObjectPool previewPool = null;
		
		protected abstract float range { get; }
		
		private LayerMask groundWallLayer;
		protected bool usedBefore = false;
		
		protected override void Awake() {
			base.Awake();
			throwPoint = transform.Find("ThrowPoint");
			//trajectoryLine = GetComponent<LineRenderer>();
			trajectoryLineMaterial = Instantiate(trajectoryLine.material);
			trajectoryLine.material = trajectoryLineMaterial;
			materialOriginalAlpha = trajectoryLineMaterial.color.a;
			if (hitRangePreviewPrefab) {
				previewPool = GameObjectPoolManager.Singleton.CreatePool(hitRangePreviewPrefab, 1, 5);
			}
			
			groundWallLayer = LayerMask.GetMask("Ground", "Wall");
			
		}

		

		protected override void OnEntityStart() {
			base.OnEntityStart();
			trajectoryLine.positionCount = 0;
			DestroySpawnedHitRangePreview();
			//animate line alpha to 0 and back to original alpha and loop
			materialTween?.Kill();
			this.RegisterEvent<OnPlayerAnimationEvent>(OnPlayerAnimationEvent)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		}

		private void OnPlayerAnimationEvent(OnPlayerAnimationEvent e) {
			if (e.AnimationName == "OnThrowGrenade") {
				UseSkill(ThrowGrenade);
			}
		}

		protected override void OnBindEntityProperty() {
			
		}

		public override void OnItemStartUse() {
			if (usedBefore) {
				return;
			}
			
			//transform.forward
			this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("HoldUse", AnimationEventType.Bool,1));

			DestroySpawnedHitRangePreview();
			trajectoryLine.positionCount = 0;
			materialTween?.Kill();
			materialTween = DOTween.Sequence()
				.Append(trajectoryLineMaterial.DOColor(
					new Color(trajectoryLineMaterial.color.r, trajectoryLineMaterial.color.g,
						trajectoryLineMaterial.color.b, 0.15f), 1f))
				.Append(trajectoryLineMaterial.DOColor(
					new Color(trajectoryLineMaterial.color.r, trajectoryLineMaterial.color.g,
						trajectoryLineMaterial.color.b, materialOriginalAlpha), 1f))
				.SetLoops(-1, LoopType.Yoyo);
		}

		private Vector3 GetThrowDirection() {
			Vector2 crossHairScreenPos = Crosshair.Singleton.CrossHairScreenPosition;

			Ray ray = Camera.main.ScreenPointToRay(crossHairScreenPos);
			RaycastHit hit;
			Vector3 throwDirection;
			if (Physics.Raycast(ray, out hit, 200, obstacleLayerMask, QueryTriggerInteraction.Ignore)) {
				throwDirection = (hit.point - throwPoint.position).normalized;
			}
			else {
				throwDirection = (ray.GetPoint(200) - throwPoint.position).normalized;
			}

			
			return throwDirection;
		}
		
		private void ShowTrajectory() {
			 Vector3 initialVelocity = GetThrowDirection() * throwForce;
		    float timeResolution = 0.02f; // Time step for the simulation, smaller step for more accuracy
		    float maxTime = 10.0f; // Max simulation time
		    Vector3 gravity = Physics.gravity;
		    Vector3 currentPosition = throwPoint.position;

		    // Define the number of positions for the backward trajectory and the forward trajectory
		    int backwardPositionsCount = 10; // Number of steps to render backwards
		    float backwardDistance = 2.0f; // Total distance to render backwards
		    int forwardPositionsCount = (int)(maxTime / timeResolution);

		    // Set the total number of positions (backward + forward)
		    trajectoryLine.positionCount = backwardPositionsCount + forwardPositionsCount;

		    // Fill the positions for the backward trajectory
		    for (int i = backwardPositionsCount - 1; i >= 0; i--) {
		        Vector3 backwardPosition = currentPosition - initialVelocity.normalized * (backwardDistance / backwardPositionsCount) * (backwardPositionsCount - i);
		        trajectoryLine.SetPosition(i, backwardPosition);
		    }

		    // Fill the positions for the forward trajectory
		    bool hitSomething = false;
		    Vector3 lastPosition = currentPosition;
		    for (int i = backwardPositionsCount; i < trajectoryLine.positionCount && !hitSomething; i++) {
		        initialVelocity += gravity * timeResolution; // Apply gravity
		        lastPosition = currentPosition; // Store the last position
		        currentPosition += initialVelocity * timeResolution; // Calculate position based on velocity

		        // Check for collision
		        if (Physics.Linecast(lastPosition, currentPosition, out RaycastHit hit, obstacleLayerMask, QueryTriggerInteraction.Ignore)) {
		            currentPosition = hit.point; // Set current position to the hit point
		            trajectoryLine.positionCount = i + 1; // Set the number of positions to the current index + 1
		            hitSomething = true; // Stop the simulation on hit
		            if (!spawnedHitRangePreview) {
			            spawnedHitRangePreview = SpawnPreviewHitRange(currentPosition);
		            }

		            if (spawnedHitRangePreview) {
			            spawnedHitRangePreview.transform.position = currentPosition;
		            
			            //rotate to normal
			            spawnedHitRangePreview.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
		            }
		          

		        }

		        trajectoryLine.SetPosition(i, currentPosition);
		    }

		    if (!hitSomething) {
			    DestroySpawnedHitRangePreview();
		    }
		    
		}


		
		public override void OnItemStopUse() {
			if (usedBefore) {
				return;
			}
			usedBefore = true;
			this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("HoldUse", AnimationEventType.Bool, 0));

			trajectoryLine.positionCount = 0;
			//ThrowGrenade();
			materialTween.Kill();
			materialTween = null;
			DestroySpawnedHitRangePreview();
			//back to 0
			//materialTween = DOTween.Sequence().Append(trajectoryLineMaterial.DOColor(
			//new Color(trajectoryLineMaterial.color.r, trajectoryLineMaterial.color.g,
			//trajectoryLineMaterial.color.b, 0), 0.2f));
		}

		private GameObject DestroySpawnedHitRangePreview() {
			if (spawnedHitRangePreview) {
				previewPool.Recycle(spawnedHitRangePreview);
				spawnedHitRangePreview = null;
			}
			return null;
		}
		
		private GameObject SpawnPreviewHitRange(Vector3 position) {
			if(previewPool == null) {
				return null;
			}
			
			spawnedHitRangePreview = previewPool.Allocate();
			spawnedHitRangePreview.transform.position = position;
			spawnedHitRangePreview.transform.localScale = new Vector3(range, 0.01f, range);
			return spawnedHitRangePreview;
		}
		
		
		private void ThrowGrenade() {
			Rigidbody grenade = Instantiate(grenadePrefab, throwPoint.position, transform.rotation)
				.GetComponent<Rigidbody>();

			List<Collider> ignoredColliders = new List<Collider>();
			ignoredColliders.AddRange(GetComponentsInChildren<Collider>(true));
			if (ownerGameObject) {
				ignoredColliders.AddRange(ownerGameObject.GetComponentsInChildren<Collider>(true));
			}
			
			OnInitThrownGrenade(grenade.gameObject, ignoredColliders.ToArray());
			
			
			Vector3 throwDirection = GetThrowDirection();
			grenade.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);

		}
		
		protected abstract void OnInitThrownGrenade(GameObject obj, params Collider[] ignoredColliders);
		

		public override void OnItemUse() {
			if (usedBefore) {
				return;
			}
			ShowTrajectory();
		}

		// public override void OnItemAltUse() { }
		
		protected override IResourceEntity OnInitSkillEntity(SkillBuilder<T> builder) {
			return builder.FromConfig().Build();
		}
		public override void OnRecycled() {
			base.OnRecycled();
			this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("HoldUse", AnimationEventType.Bool, 0));
			trajectoryLine.positionCount = 0;
			DestroySpawnedHitRangePreview();
			materialTween?.Kill();
			usedBefore = false;
		}
	}
}