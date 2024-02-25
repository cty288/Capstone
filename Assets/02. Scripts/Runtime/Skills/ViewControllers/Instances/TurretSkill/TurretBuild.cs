using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;

public class TurretBuild : Action {
	public SharedFloat installTime;
	public Renderer turretRenderer;
	
	private Material[] materials;
	private bool finished = false;
	
	public override void OnStart() {
		base.OnStart();
		Material[] mat = turretRenderer.materials;
		materials = new Material[mat.Length];
		for (int i = 0; i < mat.Length; i++) {
			materials[i] = Material.Instantiate(mat[i]);
			Color color = materials[i].GetColor("_BaseColor");
			color.a = 0;
			materials[i].SetColor("_BaseColor", color);
		}
		turretRenderer.materials = materials;
		finished = false;
		
		
		for (int i = 0; i < materials.Length; i++) {
			Color color = materials[i].GetColor("_BaseColor");
			color.a = 1;
			materials[i].DOColor(color, "_BaseColor", installTime.Value).OnComplete(() => {
				finished = true;
			});
		}
		
		
	}

	public override TaskStatus OnUpdate() {
		if (finished) {
			return TaskStatus.Success;
		}
		return TaskStatus.Running;
	}
}
