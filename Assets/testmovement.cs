using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testmovement : MonoBehaviour
{
    private Rigidbody rb;
    public int moveSpeed;
    // Start is called before the first frame update
    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            Vector3 movement = -transform.forward * moveSpeed * Time.deltaTime;

            // Use Translate to move the object.
            transform.Translate(movement);
            
        }
        {
            if (Input.GetKey(KeyCode.D))
            {

                Vector3 movement = transform.forward * moveSpeed * Time.deltaTime;

                // Use Translate to move the object.
                transform.Translate(movement);
            }
        }
    }
}
