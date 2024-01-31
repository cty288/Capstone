using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorrectiveBlendshape : MonoBehaviour
{
    [SerializeField] private Transform corrector;
    private SkinnedMeshRenderer _skin;

    private void Start()
    {
        _skin = GetComponent<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        _skin.SetBlendShapeWeight(0, 100*Vector3.Dot(-corrector.transform.right, Vector3.down));
    }
}
