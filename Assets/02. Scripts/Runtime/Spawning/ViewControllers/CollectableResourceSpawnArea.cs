using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.CollectableResources.Model.Properties;
using _02._Scripts.Runtime.CollectableResources.ViewControllers.Base;
using _02._Scripts.Runtime.Utilities;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.GameResources;
using Runtime.Spawning;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


[Serializable]
public class CollectableResourceSpawnGroup {
    public int weight;
    public List<GameObject> prefabVariants;
    public int maxAngle = 45;
}

[RequireComponent(typeof(BoxCollider))]
public class CollectableResourceSpawnArea : MonoBehaviour {
    [SerializeField]
    private List<CollectableResourceSpawnGroup> spawnPrefabGroups;
    
    [SerializeField] 
    private float perlinNoiseThreshold = 0.7f;

    [SerializeField] 
    private int minSpawnCount = 5;
    [SerializeField]
    private int maxSpawnCount = 10000;

    [SerializeField] 
    private Vector2 spawnNormalStepRange = new Vector2(1f, 3f);

    [SerializeField] 
    private float accuracyTolerance = 10;

    private BoxCollider boxCollider;
    private List<List<SafeGameObjectPool>> safeGameObjectPools = new List<List<SafeGameObjectPool>>();
    private void Awake() {
        boxCollider = GetComponent<BoxCollider>();
        foreach (CollectableResourceSpawnGroup group in spawnPrefabGroups) {
            List<SafeGameObjectPool> pools = new List<SafeGameObjectPool>();
            safeGameObjectPools.Add(pools);
            foreach (GameObject prefabVariant in group.prefabVariants) {
                pools.Add(GameObjectPoolManager.Singleton.CreatePool(prefabVariant, 10, 50));
            }
        }
    }

    private void Update() {
        //if (Input.GetKeyDown(KeyCode.P)) {
          //  Spawn(Random.Range(0f, 10000000f));
        //}
    }

    private CollectableResourceSpawnGroup GetRandomSpawnGroup(out int groupIndex) {
        int totalWeight = 0;
        foreach (CollectableResourceSpawnGroup group in spawnPrefabGroups) {
            totalWeight += group.weight;
        }

        int randomWeight = Random.Range(0, totalWeight);
        int currentWeight = 0;
        for (int i = 0; i < spawnPrefabGroups.Count; i++) {
            currentWeight += spawnPrefabGroups[i].weight;
            if (randomWeight < currentWeight) {
                groupIndex = i;
                return spawnPrefabGroups[i];
            }
        }

        groupIndex = 0;
        return spawnPrefabGroups[0];
    }

    public void Spawn(float seed) {

        int count = 0;
        Bounds bounds = boxCollider.bounds;
        float xMin = bounds.min.x;
        float xMax = bounds.max.x;
        float zMin = bounds.min.z;
        float zMax = bounds.max.z;
        
        float xStep = Random.Range(spawnNormalStepRange.x, spawnNormalStepRange.y);
        float zStep = Random.Range(spawnNormalStepRange.x, spawnNormalStepRange.y);

        float y = bounds.center.y;
        var insideArenaRefPoints =
            GameObject.FindGameObjectsWithTag("ArenaRefPoint").Select(x => x.transform.position).ToArray();
        
        HashSet<Vector3> usedPositions = new HashSet<Vector3>();

        for (float x = xMin; x < xMax; x += xStep) {
            for (float z = zMin; z < zMax; z += zStep) {
                
                xStep = Random.Range(spawnNormalStepRange.x, spawnNormalStepRange.y);
                zStep = Random.Range(spawnNormalStepRange.x, spawnNormalStepRange.y);
                
                CollectableResourceSpawnGroup spawnGroup = GetRandomSpawnGroup(out int groupIndex);
                
                int prefabIndex = Random.Range(0, spawnGroup.prefabVariants.Count);
                GameObject prefab = spawnGroup.prefabVariants[prefabIndex];
                ICollectableResourceViewController collectableResourceViewController =
                    prefab.GetComponent<ICollectableResourceViewController>();
                
                /*Vector3 spawnPos = SpawningUtility.FindNavMeshSuitablePosition(
                    () => collectableResourceViewController.SpawnSizeCollider,
                    new Vector3(x, y, z),
                    spawnGroup.maxAngle,
                    NavMeshHelper.GetSpawnableAreaMask(),
                    insideArenaRefPoints,
                    5f,
                    1f,
                    1,
                    out int usedAttempts,
                    out Quaternion rotationWithSlope
                );*/

                Vector3 spawnPos = SpawningUtility.FindNavMeshSuitablePositionFast(
                    new Vector3(x, y, z),
                    NavMeshHelper.GetSpawnableAreaMask(),
                    accuracyTolerance,
                    1f,
                    1,
                    out _,
                    out Quaternion rotationWithSlope
                );
                
                

                if (float.IsInfinity(spawnPos.magnitude) || usedPositions.Contains(spawnPos)) {
                    continue;
                }
                
                
                //x = spawnPos.x;
                //z = spawnPos.z;

                float noise = PerlinNoise(spawnPos.x, y, spawnPos.z, seed);
                if (noise < perlinNoiseThreshold) {
                    continue;
                }

                usedPositions.Add(spawnPos);
                GameObject spawnedInstance = safeGameObjectPools[groupIndex][prefabIndex].Allocate();
                spawnedInstance.transform.position = spawnPos;
              
                

                spawnedInstance.transform.rotation = rotationWithSlope;
                //random rotate around y axis
                spawnedInstance.transform.Rotate(Vector3.up, Random.Range(0, 360));
                spawnedInstance.transform.SetParent(transform.parent);

                Bounds spawnBounds = spawnedInstance.GetComponent<ICollectableResourceViewController>()
                    .SpawnSizeCollider.bounds;
                //move them a little down, randomize between 0.2 to 0.6 extent y
                spawnedInstance.transform.position -= Vector3.up * (Random.Range(0.2f, 0.6f) * spawnBounds.extents.y);
                
                xStep = Random.Range(spawnBounds.extents.x, spawnBounds.extents.x * 2);
                zStep = Random.Range(spawnBounds.extents.z, spawnBounds.extents.z * 2);
                count++;
                
                if (count >= maxSpawnCount) {
                    return;
                }
                
            }
        }
        
        if (count < minSpawnCount) {
            //randomly sample a position
            for (int i = 0; i < minSpawnCount - count; i++) {
                Vector3 randomPos = new Vector3(Random.Range(xMin, xMax), y, Random.Range(zMin, zMax));
                CollectableResourceSpawnGroup spawnGroup = GetRandomSpawnGroup(out int groupIndex);
                
                int prefabIndex = Random.Range(0, spawnGroup.prefabVariants.Count);
                GameObject prefab = spawnGroup.prefabVariants[prefabIndex];
                ICollectableResourceViewController collectableResourceViewController =
                    prefab.GetComponent<ICollectableResourceViewController>();
                
                /*Vector3 spawnPos = SpawningUtility.FindNavMeshSuitablePosition(
                    () => collectableResourceViewController.SpawnSizeCollider,
                    randomPos,
                    45,
                    NavMeshHelper.GetSpawnableAreaMask(),
                    null,
                    50f,
                    1f,
                    1,
                    out int usedAttempts,
                    out Quaternion rotationWithSlope
                );*/
                Vector3 spawnPos = SpawningUtility.FindNavMeshSuitablePositionFast(
                    randomPos,
                    NavMeshHelper.GetSpawnableAreaMask(),
                    accuracyTolerance,
                    1f,
                    1,
                    out _,
                    out Quaternion rotationWithSlope
                );
                
                if (float.IsInfinity(spawnPos.magnitude)) {
                    continue;
                }
                
                //x = spawnPos.x;
                //z = spawnPos.z;
                
                GameObject spawnedInstance = safeGameObjectPools[groupIndex][prefabIndex].Allocate();
                spawnedInstance.transform.position = spawnPos;
                spawnedInstance.transform.rotation = rotationWithSlope;
                //random rotate around y axis
                spawnedInstance.transform.Rotate(Vector3.up, Random.Range(0, 360));
                spawnedInstance.transform.SetParent(transform.parent);
            }
        }
    }


    private float PerlinNoise(float x, float y, float z, float seed) {
        float noise = Perlin.Noise(x + seed, y + seed, z + seed);
        //map noise from [-1, 1] to [0, 1]
        noise = (noise + 1) / 2;
        return noise;
    }
}
