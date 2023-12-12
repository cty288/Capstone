using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.ResKit;
using UnityEngine;

public class MoveBakedMeshRenderObject : DefaultPoolableGameObject {
	private float delTime = 0.8f;
	private float delSpeed = 1;
	private Material mat;
	private Color color;
	MeshRenderer meshRenderer;
	MeshFilter meshFilter;
	private Material originalMat;
	private void Awake() {
		meshRenderer = GetComponent<MeshRenderer>();
		meshFilter = GetComponent<MeshFilter>();
		originalMat = meshRenderer.material;
	}
	
	public void Initialize(float transparency, float delSpeed, Color color, float globalScale, Mesh mesh, Material overrideMat = null) {
		this.delTime = transparency;
		this.delSpeed = delSpeed;
		this.color = color;
		transform.localScale = Vector3.one * globalScale;
		if (!overrideMat) {
			meshRenderer.material = Instantiate(originalMat);
		}
		else {
			meshRenderer.material = Instantiate(overrideMat);
		}
		
		mat = meshRenderer.material;
		meshFilter.mesh = mesh;
	}

	void Update() {
		if (IsRecycled) {
			return;
		}
		delTime -= Time.deltaTime * delSpeed;
		if (mat) {
			Color c = color;
			c.a = delTime / 4;
			mat.SetColor("_BaseColor", c);
		}

		if (delTime <= 0) {
			RecycleToCache();
		}
	}

	public override void OnRecycled() {
		base.OnRecycled();
		delTime = 0.8f;
		delSpeed = 1;
	}
}
