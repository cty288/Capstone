using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Weapons
{
    public class GunRecoil : MonoBehaviour
    {
        private Vector3 currentRotation;
        private Vector3 targetRotation;

        [SerializeField] private float recoilX;
        [SerializeField] private float recoilY;
        [SerializeField] private float recoilZ;

        [SerializeField] private float snappiness;
        [SerializeField] private float returnSpeed;

        private void Awake()
        {
            // this.RegisterEvent<OnCurrentHotbarUpdateEvent>(OnCurrentHotbarUpdate)
            //     .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void Update()
        {
            targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
            currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(currentRotation);
        }

        public void RecoilFire()
        {
            targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
        }

     
    }
}