using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class RangedAOE : EnemyAction
{
    public GameObject bulletPrefab;
    public int bulletCount;
    public int spawnInterval;
    private bool ended;
    public float bulletTravelTime;

    public SharedTransform playerTrans;

    public override void OnStart()
    {
        ended = false;
        StartCoroutine(RF());
    }
    public override TaskStatus OnUpdate()
    {
        if (ended)
            return TaskStatus.Success;
        else
            return TaskStatus.Running;

    }
    IEnumerator RF()
    {
        for (int i = 0; i < bulletCount; i++)
        {
            SpawnBullet();
            yield return new WaitForSeconds(spawnInterval);
        }
        ended = true;
    }
    void SpawnBullet()
    {
        GameObject b = Object.Instantiate(bulletPrefab, new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), Quaternion.identity);
        b.GetComponent<EnemyBomb>().Init(playerTrans.Value, bulletTravelTime);


    }
}
