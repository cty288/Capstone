using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockFPS : MonoBehaviour
{
    [SerializeField]
    private int lockedFPS = 60;

    private void Awake() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = lockedFPS;
    }
}
