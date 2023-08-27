using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Enemies;
using Runtime.Utilities.Collision;

namespace Runtime.Enemies
{
    public class Boss1_HurtResponder : MonoBehaviour, IHurtResponder
    {
        public Boss1 boss1MVC;
        public bool CheckHurt(HitData data)
        {
            throw new System.NotImplementedException();
        }

        public void HurtResponse(HitData data)
        {
            throw new System.NotImplementedException();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

