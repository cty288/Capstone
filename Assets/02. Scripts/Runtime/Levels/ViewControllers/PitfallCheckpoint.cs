using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitfallCheckpoint : MonoBehaviour
{
    private Pitfall _pitfall;
    private void Start()
    {
        _pitfall = transform.parent.GetComponentInChildren<Pitfall>();
        if (_pitfall == null)
        {
            Debug.LogWarning("Could not find associated pitfall");
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            Debug.Log("Checkpoint");
            _pitfall.currentRespawn = this;
        }
    }
}
