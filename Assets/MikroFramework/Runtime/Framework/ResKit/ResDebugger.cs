using System.Collections;
using MikroFramework.Pool;
using UnityEngine;

namespace MikroFramework.ResKit {
	public class ResDebugger : MonoBehaviour {
#if UNITY_EDITOR
		private void OnGUI()
		{
			if (Input.GetKey(KeyCode.F2))
			{
				GUILayout.BeginVertical("box");

				IEnumerator valueEnumerator = ResManager.singleton.SharedLoadedResource.Items.GetEnumerator();

				while (valueEnumerator.MoveNext()) {
					Res loadedRes = valueEnumerator.Current as Res;
					GUILayout.Label($"Name: {loadedRes.Name}, ResCount: {loadedRes.RefCount}, ResState: {loadedRes.State}");
				}
                

				GUILayout.Label($"ResourceRes Object Pool count: {SafeObjectPool<ResourcesRes>.Singleton.CurrentCount}" +
				                $"/{SafeObjectPool<ResourcesRes>.Singleton.MaxCount}. " +
				                $"Active object count: {SafeObjectPool<ResourcesRes>.Singleton.NumActiveObject.Value}");

				GUILayout.Label($"AssetBundleRes Object Pool count: {SafeObjectPool<AssetBundleRes>.Singleton.CurrentCount}" +
				                $"/{SafeObjectPool<AssetBundleRes>.Singleton.MaxCount}. " +
				                $"Active object count: {SafeObjectPool<AssetBundleRes>.Singleton.NumActiveObject.Value}");

				GUILayout.Label($"AssetRes Object Pool count: {SafeObjectPool<AssetRes>.Singleton.CurrentCount}" +
				                $"/{SafeObjectPool<AssetRes>.Singleton.MaxCount}. " +
				                $"Active object count: {SafeObjectPool<AssetRes>.Singleton.NumActiveObject.Value}");

				GUILayout.EndVertical();
			}

		}
#endif
	}
}