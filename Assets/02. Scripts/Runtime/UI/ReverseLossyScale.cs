using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseLossyScale : MonoBehaviour
{
    private Vector3 _lossyScale;
    void FixedUpdate()
    {
        if(_lossyScale.magnitude != transform.localScale.magnitude){
            _lossyScale = transform.lossyScale;
            if (Math.Abs(_lossyScale.x - 1.0f) > 0.01f)
            {
                transform.localScale = new Vector3(1/_lossyScale.x, 1/_lossyScale.y, 1/_lossyScale.z);
            }
        }
    }
}
