using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class forward : MonoBehaviour
{
    private float timer = 5f;
    public GameObject prefab;
    // Start is called before the first frame update
   

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Vector3 sample = Random.insideUnitSphere * 30;
            Vector3 sample2 = Random.insideUnitSphere * 30;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(sample, out hit, 15, 1))
            {
                Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), hit.position, Quaternion.identity);
            }
            if (NavMesh.SamplePosition(sample2, out hit, 15, 1))
            {
                Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), hit.position, Quaternion.identity);
            }
        }
        if(timer > 0)
        {
            timer -= Time.deltaTime;
            this.gameObject.transform.Translate(this.transform.forward * 2 * Time.deltaTime);
            this.gameObject.transform.Translate(this.transform.up * 5 * Time.deltaTime);
            
        }
    }
}
