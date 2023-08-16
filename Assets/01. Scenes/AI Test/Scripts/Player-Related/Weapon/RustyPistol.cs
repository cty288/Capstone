using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;

public class RustyPistol : WeaponEntity<RustyPistol>
{
    public override string EntityName { get; protected set; } = "RustyPistol";

    public override void OnRecycle() { }

    protected override ICustomProperty[] OnRegisterCustomProperties()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnWeaponRegisterProperties()
    {
        throw new System.NotImplementedException();
    }
}