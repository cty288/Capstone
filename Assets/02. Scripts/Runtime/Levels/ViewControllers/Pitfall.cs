using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Commands;
using Framework;
using MikroFramework.Architecture;
using UnityEngine;

public class Pitfall : AbstractMikroController<MainGame>
{
    public PitfallCheckpoint currentRespawn;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            //teleport the player
            Debug.Log("Respawn Hit!");
            this.SendCommand<TeleportPlayerCommand>(
                TeleportPlayerCommand.Allocate(currentRespawn.transform.position));
        }
    }

   
}
