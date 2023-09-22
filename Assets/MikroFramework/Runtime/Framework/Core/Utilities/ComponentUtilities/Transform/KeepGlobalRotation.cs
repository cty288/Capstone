using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Mikrocosmos{
    public class KeepGlobalRotation : MonoBehaviour{
        [SerializeField]
        private Vector3 rotation;

        [SerializeField] private Transform positionRelativeTo;

        [SerializeField]
        private Vector3 positionOffset;

        public Vector3 PositionOffset {
            get => positionOffset;
            set => positionOffset = value;
        }
        private void OnEnable() {
            //positionOffset = positionRelativeTo.position - transform.position;

        }

        private void Update() {
            transform.rotation = Quaternion.Euler(rotation);
            if (positionRelativeTo) {
                transform.position = positionRelativeTo.position + positionOffset;
            }
         
        }
    }
}
