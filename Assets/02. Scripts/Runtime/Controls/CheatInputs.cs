using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Controls;
using UnityEngine;

public class CheatInputs : MonoBehaviour
{
    private DPunkInputs.DebugActions debugActions;

    private void Awake()
    {
        debugActions = ClientInput.Singleton.GetDebugActions();
    }

    void Update()
    {
        if (debugActions.SlowTime.WasPressedThisFrame())
        {
            Time.timeScale = 0.05f;
        }

        if (debugActions.SlowTime.WasReleasedThisFrame())
        {
            Time.timeScale = 1f;
        }
    }
}
