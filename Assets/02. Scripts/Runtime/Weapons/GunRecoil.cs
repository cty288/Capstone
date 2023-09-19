using System;
using Framework;
using MikroFramework.Architecture;
using Runtime.Weapons.Model.Base;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Weapons
{
    public class GunRecoil : AbstractMikroController<MainGame>
    {
        private Vector3 currentRotation;
        private Vector3 targetRotation;

         //negative is upwards, positive downwards
        [SerializeField] private float recoilX;
        //positive to right
        [SerializeField] private float recoilY;
        //screen shake
        [SerializeField] private float recoilZ;

        [SerializeField] private float snappiness;
        [SerializeField] private float returnSpeed;

        private void Start()
        {
            this.RegisterEvent<OnWeaponRecoilEvent>(Recoil);
            // Debug.Log("register recoil");
        }

        private void Update()
        {
            targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
            currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(currentRotation);
        }

        public void RecoilFire()
        {
            // targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ)); // original
            targetRotation += new Vector3(recoilX, Random.Range(0, recoilY), Random.Range(-recoilZ, recoilZ)); // 0 to number
            // targetRotation += new Vector3(recoilX, recoilY, recoilZ); // exact numbers
        }

        private void Recoil(OnWeaponRecoilEvent e)
        {
            Debug.Log("recoil");
            RecoilFire();
        }
    }
}