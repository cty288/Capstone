using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MikroFramework.Utilities;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Player;
using Runtime.Temporary;
using Runtime.DataFramework.ViewControllers.Entities;
using System;

namespace Runtime.Utilities.Collision
{
    public class ViralHitBox : MonoBehaviour
{
        float checkForHit = 1f;
        bool canHit = true;
        private void Update()
        {
            if (canHit == false)
            {

                checkForHit -= Time.deltaTime;
            }
            if(checkForHit < 0)
            {
                canHit = true;
                checkForHit = 1f;

            }
        }


        private void OnTriggerStay(Collider other)
        {
           if(other.gameObject.transform.parent.tag == "Player")
            {
                Debug.Log("true1");
                var a = other.gameObject.transform.parent.GetComponent<PlayerController>().GetPlayerEntity();
                if(a != null)
                {
                    if(canHit == true)
                    {

                        canHit = false;
                        Debug.Log("true2");
                        HitData h = new HitData();
                        h.SetHitBoxData(null, 5, null, Vector3.zero, Vector3.zero, null, false);
                        a.TakeDamage(70, null, out _, null);
                    }
                }
            }
        }
    }

 
}
