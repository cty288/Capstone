using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using Runtime.Utilities.Collision;
using UnityEngine;
using Runtime.Temporary.Weapon;
using Runtime.Weapons.ViewControllers.Base;
using Cinemachine;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormAwake : EnemyAction
    {
        public Material mat;
        public float intensity;
        public Color targetEmissionColor = new Color(191, 1, 40); // Set the target emission color in the Inspector.
        public float lerpSpeed = 3.0f; // Set the lerping speed in seconds.
        public float timer = 3f;

        private Color startEmissionColor;

        // Start is called before the first frame update
        public override void OnStart()
        {
            mat = this.gameObject.transform.GetChild(0).GetComponent<Renderer>().material;
            startEmissionColor = mat.GetColor("_EmissionColor");
        }

        // Update is called once per frame
        public override TaskStatus OnUpdate()
        {
            timer -= Time.deltaTime;
            // Calculate the current emission color by lerping.
            Color currentEmissionColor = Color.Lerp(startEmissionColor, targetEmissionColor * intensity, Time.deltaTime / lerpSpeed);

            // Set the new emission color on the material.
            mat.SetColor("_EmissionColor", targetEmissionColor * intensity);
            if(timer < 0)
            {
                timer = 1;
                return TaskStatus.Success;
            }
            return TaskStatus.Running;
        }
    }
}