using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class forward : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.Translate(this.transform.forward * 5 * Time.deltaTime);
        this.gameObject.transform.Translate(this.transform.up * 5 * Time.deltaTime);
    }
}
