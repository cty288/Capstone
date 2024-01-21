using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.ViewControllers.Entities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public class PrefabPostProcessor : AssetPostprocessor
{
	private static string markerFolder = "Assets/06. Prefabs/Markers/";
	
	[MenuItem("Tools/Update All Prefabs IDs")]
	public static void UpdatePrefabIDs()
	{
		
		string rootPath = "Assets/";

		
		string[] allPrefabs = Directory.GetFiles(rootPath, "*.prefab", SearchOption.AllDirectories);

		foreach (string prefabPath in allPrefabs) {
			UpdatePrefabIDAtPath(prefabPath);
		}

		AssetDatabase.SaveAssets(); 
	}

	private static void UpdatePrefabIDAtPath(string str) {
		if (str.EndsWith(".prefab"))
		{
			Debug.Log("Prefab imported: " + str);
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(str);
                
			if (prefab != null) {
				IEntityViewController prefabIdentifier = prefab.GetComponent<IEntityViewController>();
				if (prefabIdentifier == null)
				{
					return;
				}
					
				string pathGuid = AssetDatabase.AssetPathToGUID(str);
				string markerFilePath = markerFolder + pathGuid + ".marker";
				

				if (!File.Exists(markerFilePath)) {
					prefabIdentifier.PrefabID = Guid.NewGuid().ToString();
					File.Create(markerFilePath).Dispose();
					EditorUtility.SetDirty(prefab);
					AssetDatabase.SaveAssets();
				}
				else
				{
					if (string.IsNullOrEmpty(prefabIdentifier.PrefabID)) {
						prefabIdentifier.PrefabID = Guid.NewGuid().ToString();
						EditorUtility.SetDirty(prefab);
						AssetDatabase.SaveAssets();
					}
				}
			}
		}
	}
	
	/*private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
		string[] movedFromAssetPaths) {
		foreach (string str in importedAssets) {
			UpdatePrefabIDAtPath(str);
		}
	}*/

	/*private void OnPostprocessPrefab(GameObject g)
	{
		
		Debug.Log("Prefab imported: " + g.name);
		//GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(g);
                
		if (g != null)
		{
			IEntityViewController prefabIdentifier = g.GetComponent<IEntityViewController>();
			if (prefabIdentifier == null) {
				return;
			}

			
			string path = AssetDatabase.GetAssetPath(g); 
			Debug.Log("Prefab path: " + path);

			if (string.IsNullOrEmpty(prefabIdentifier.PrefabID)) {
				prefabIdentifier.PrefabID = Guid.NewGuid().ToString();
				EditorUtility.SetDirty(g); // Mark prefab for saving
				AssetDatabase.SaveAssets(); // Save changes
			}
		}
	}*/
	
	
}

