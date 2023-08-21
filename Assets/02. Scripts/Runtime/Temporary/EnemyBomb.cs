using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBomb : MonoBehaviour
{
    private Vector3 targetPos;
    public float travelTime;
    private float startTime;
    private bool slerping;
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (slerping)
        {
            float fracComplete = (Time.time - startTime) / travelTime;
            transform.position = Vector3.Slerp(transform.position, targetPos, fracComplete);
            if (Vector3.Distance(transform.position,targetPos) <= 1)
            {
                slerping = false;
            }
        }
        
    }
    public void Init(Transform target,float tTime)
    {
        slerping = true;
        targetPos = target.position;
        startTime = Time.time;
        travelTime = tTime;
    }
}
