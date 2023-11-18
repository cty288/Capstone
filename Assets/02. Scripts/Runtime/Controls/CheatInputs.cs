using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using Runtime.Controls;
using UnityEngine;

public class CheatInputs : MonoBehaviour, ICanGetModel
{
    private DPunkInputs.DebugActions debugActions;
    private IPlayerModel playerModel;

    private void Awake()
    {
        debugActions = ClientInput.Singleton.GetDebugActions();
        playerModel = this.GetModel<PlayerModel>();
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

        if (debugActions.Heal.WasPressedThisFrame())
        {
            playerModel.HP.Value += 50;
        }
    }

    public IArchitecture GetArchitecture()
    {
        return MainGame.Interface;
    }
}
