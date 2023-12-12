using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using UnityEngine;

public interface IPlayerModel : IModel {
    BindableProperty<int> HP { get; }
    
    BindableProperty<int> CurrentSubAreaMask { get; }
}

public class PlayerModel : AbstractModel, IPlayerModel {
    protected override void OnInit() {
        
    }

    public BindableProperty<int> HP { get; } = new BindableProperty<int>() {Value = 3};
    public BindableProperty<int> CurrentSubAreaMask { get; } = new BindableProperty<int>() {Value = -1};
}
