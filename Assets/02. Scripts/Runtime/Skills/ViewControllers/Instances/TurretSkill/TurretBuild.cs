using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class TurretBuild : Action {
	public SharedFloat installTime;
	public Renderer[] turretRenderers;
	
	private List<Material> materials;
	private bool finished = false;
	private Animator animator;
	private TaskStatus taskStatus = TaskStatus.Running;

	public override void OnAwake() {
		base.OnAwake();
		animator = gameObject.GetComponentInChildren<Animator>();
	}

	public override void OnStart()
	{
		base.OnStart();
		// IncreaseTransparency();
		foreach (Renderer turretRenderer in turretRenderers)
		{
			turretRenderer.enabled = false;
		}

		SkillExecute();
	}

	private async void SkillExecute()
	{
		float buildInterval = installTime.Value / turretRenderers.Length;
		foreach (Renderer turretRenderer in turretRenderers)
		{
			turretRenderer.enabled = true;
			await UniTask.WaitForSeconds(buildInterval, cancellationToken: gameObject.GetCancellationTokenOnDestroy());
		}
		
		taskStatus = TaskStatus.Success;
	}
	
	private void IncreaseTransparency()
	{
		materials = new List<Material>();
		animator.SetBool("isShooting", false);
		foreach (Renderer turretRenderer in turretRenderers) {
			Material[] mat = turretRenderer.materials;
			Material[] rendererMaterials = new Material[mat.Length];
			for (int i = 0; i < mat.Length; i++) {
				Material material = Material.Instantiate(mat[i]);
				Color color = material.GetColor("_BaseColor");
				color.a = 0;
				material.SetColor("_BaseColor", color);
				materials.Add(material);
				rendererMaterials[i] = material;
			}
			turretRenderer.materials = rendererMaterials;
		}
		
		finished = false;
		
		
		for (int i = 0; i < materials.Count; i++) {
			Color color = materials[i].GetColor("_BaseColor");
			color.a = 1;
			materials[i].DOColor(color, "_BaseColor", installTime.Value).OnComplete(() => {
				finished = true;
			});
		}
	}

	public override TaskStatus OnUpdate() {
		return taskStatus;
	}
}
