using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;


public class Meteor : EnemyAction
{
    public BoxCollider spawnArea;
    public GameObject meteorPrefab;
    public int spawnCount;
    public float spawnInterval;
    public Wait wait;

    public override TaskStatus OnUpdate()
    {
        var sequence = DOTween.Sequence();
        for(int i = 0; i < spawnCount; i++)
        {
            sequence.AppendCallback(SpawnMeteor);
            sequence.AppendInterval(spawnInterval);
        }
        wait.waitTime = spawnInterval * spawnCount;
        return TaskStatus.Success;

    }

    private void SpawnMeteor()
    {
        var randomX = Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x);
        var randomZ = Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z);

        var meteor = Object.Instantiate(meteorPrefab, new Vector3(randomX, spawnArea.bounds.min.y, randomZ), Quaternion.identity);
    }
}
