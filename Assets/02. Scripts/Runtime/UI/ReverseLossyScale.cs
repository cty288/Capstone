using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseLossyScale : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        var lossyScale = transform.lossyScale;
        transform.localScale = new Vector3(1/lossyScale.x, 1/lossyScale.y, 1/lossyScale.z);
    }
}
