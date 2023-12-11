using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UppercutBladesIndicator : MonoBehaviour {
    private ParticleSystem[] particles;

    private void Awake() {
        particles = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void Hide() {
        foreach (ParticleSystem particle in particles) {
            ParticleSystem.MainModule module = particle.main;

            NavMeshHit hit;
            NavMesh.SamplePosition(transform.position, out hit, 100f, NavMesh.AllAreas);
            

            //module.loop = true;
        }
    }
}
