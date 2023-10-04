using System;
using Framework;
using MikroFramework.Architecture;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Model;
using Runtime.Weapons.Model.Base;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Weapons
{
    public class GunAmmoVisual : AbstractMikroController<MainGame>
    {
        [SerializeField] private GameObject weapon;
        private IWeaponEntity _associatedWeapon;
        [SerializeField] private Material gunBarrelIndicator;
        private Material _instanceGunBarrelIndicator;
        private static readonly int MaxAmmo = Shader.PropertyToID("_MaxAmmo");
        private static readonly int CurrentAmmo = Shader.PropertyToID("_CurrentAmmo");

        // Start is called before the first frame update
        void Start()
        {
            _associatedWeapon = weapon.GetComponent<IResourceViewController>().Entity as IWeaponEntity;
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer.material == null)
            {
                renderer.material = new Material(gunBarrelIndicator);
            }
            _instanceGunBarrelIndicator = renderer.material;
            _associatedWeapon.CurrentAmmo.RegisterWithInitValue(OnAmmoChanged);
            _instanceGunBarrelIndicator.SetInteger(MaxAmmo, _associatedWeapon.GetAmmoSize().RealValue);
        }

        public void OnAmmoChanged(int num)
        {
            _instanceGunBarrelIndicator.SetFloat(CurrentAmmo, num);
        }

        private void OnDestroy()
        {
            _associatedWeapon.CurrentAmmo.UnRegisterOnValueChanged(OnAmmoChanged);
        }
    }
}
