using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Runtime.DataFramework.Entities;

namespace Runtime.DataFramework.ViewControllers
{
    public abstract class AbstractWeaponViewController<T> : AbstractHaveCustomPropertyEntityViewController<T, IWeaponEntityModel>, IWeaponViewController
                where T : class, IWeaponEntity, new()
    {
    }

}
