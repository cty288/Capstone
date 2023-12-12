using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using UnityEngine;

public class MoveBakeMesh : AbstractMikroController<MainGame> {
	//[SerializeField]
	private SkinnedMeshRenderer[] skinList;
	[SerializeField]
	public int spaceTime = 5;
	[SerializeField]
	private float initialTransparency = 0.8f;
	[SerializeField]
	private float fadeSpeed = 1;
	[SerializeField]
	private Color color = new Color(1f, 0.2f, 0, 1);
	[SerializeField]
	private float globalScale = 1;
	[SerializeField]
	private Material overrideMat = null;

	[SerializeField] private float minDistance = 0.1f;
	
	
	private int countTime = 0;
	private Vector3 oldPos;

	private SafeGameObjectPool pool;
	// Start is called before the first frame update
	void Awake() {
		skinList = this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		pool = GameObjectPoolManager.Singleton.CreatePoolFromAB("MoveBasedMeshObj", null, 50, 200, out _);
		
	}

	// Update is called once per frame
	void Update() {
		countTime++;
		if(countTime % spaceTime == 0)
		{
			BakeFun();
		}
	}

	private void BakeFun()
	{
		if(Vector3.Distance(this.gameObject.transform.position,oldPos) > minDistance) {
			oldPos = this.gameObject.transform.position;
			for (int i = 0; i < skinList.Length; i++) {
				Mesh mesh = new Mesh();
				skinList[i].BakeMesh(mesh);
				GameObject go = pool.Allocate();
				MoveBakedMeshRenderObject mbm = go.GetComponent<MoveBakedMeshRenderObject>();

				mbm.Initialize(initialTransparency, fadeSpeed, color, globalScale, mesh, overrideMat);
				
				go.transform.position = this.transform.position;
				go.transform.rotation = this.transform.rotation;
				
			}
		}

	}
}
