using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Baits.Model.Base;
using _02._Scripts.Runtime.Levels.Models;
using MikroFramework.Architecture;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using UnityEngine;

public class BaitDeployedViewController : AbstractDeployableResourceViewController<BaitEntity> {
	private ILevelModel levelModel;

	protected override void Awake() {
		base.Awake();
		levelModel = this.GetModel<ILevelModel>();
	}

	protected override void OnEntityStart() {
		
	}

	protected override void OnBindEntityProperty() {
		
	}

	protected override IResourceEntity OnBuildNewEntity(bool isPreview) {
		return null;
	}

	public override bool CheckCanDeploy(Vector3 slopeNormal, Vector3 position, bool isAir, out DeployFailureReason failureReason,
		out Quaternion spawnedRotation) {
		if (levelModel.CurrentLevel.Value.IsInBattle()) {
			failureReason = DeployFailureReason.BaitInBattle;
			spawnedRotation = Quaternion.identity;
			return false;
		}
		return base.CheckCanDeploy(slopeNormal, position, isAir, out failureReason, out spawnedRotation);
	}
}
