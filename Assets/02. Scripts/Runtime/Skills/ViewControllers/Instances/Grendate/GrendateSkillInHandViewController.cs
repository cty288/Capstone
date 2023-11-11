using _02._Scripts.Runtime.Skills.Model.Builders;
using _02._Scripts.Runtime.Skills.Model.Instance;
using _02._Scripts.Runtime.Skills.ViewControllers.Base;
using DG.Tweening;
using Runtime.GameResources.Model.Base;
using Runtime.Weapons.ViewControllers;
using UnityEngine;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances {
	public class GrendateSkillInHandViewController : AbstractInHandSkillViewController<GrenadeSkill> {
		[SerializeField]
		private GameObject grenadePrefab;

		[SerializeField]
		private float throwForce = 30f;
		
		[SerializeField]
		private LayerMask obstacleLayerMask;
		[SerializeField]
		private LineRenderer trajectoryLine;
		private Transform throwPoint;
		private Material trajectoryLineMaterial;
		private Tween materialTween;
		private float materialOriginalAlpha;

		protected override void Awake() {
			base.Awake();
			throwPoint = transform.Find("ThrowPoint");
			//trajectoryLine = GetComponent<LineRenderer>();
			trajectoryLineMaterial = Instantiate(trajectoryLine.material);
			trajectoryLine.material = trajectoryLineMaterial;
			materialOriginalAlpha = trajectoryLineMaterial.color.a;
		}

		protected override void OnEntityStart() {
			base.OnEntityStart();
			trajectoryLine.positionCount = 0;
			//animate line alpha to 0 and back to original alpha and loop
			materialTween?.Kill();

		}

		protected override void OnBindEntityProperty() {
			
		}

		public override void OnItemStartUse() {
			//transform.forward
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
		        }

		        trajectoryLine.SetPosition(i, currentPosition);
		    }
		}


		public override void OnItemStopUse() {
			trajectoryLine.positionCount = 0;
			ThrowGrenade();
			materialTween.Kill();
			materialTween = null;
			//back to 0
			//materialTween = DOTween.Sequence().Append(trajectoryLineMaterial.DOColor(
				//new Color(trajectoryLineMaterial.color.r, trajectoryLineMaterial.color.g,
					//trajectoryLineMaterial.color.b, 0), 0.2f));
		}

		private void ThrowGrenade() {
			Rigidbody grenade = Instantiate(grenadePrefab, throwPoint.position, transform.rotation)
				.GetComponent<Rigidbody>();

			Vector3 throwDirection = GetThrowDirection();
			grenade.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
		}

		public override void OnItemUse() {
			ShowTrajectory();
		}

		protected override IResourceEntity OnInitSkillEntity(SkillBuilder<GrenadeSkill> builder) {
			return builder.FromConfig().Build();
		}

		public override void OnRecycled() {
			base.OnRecycled();
			trajectoryLine.positionCount = 0;
			materialTween?.Kill();
		}
	}
}