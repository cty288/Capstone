using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Utilities.Collision;
using UnityEngine;

public class HurtboxModifier : MonoBehaviour
{
    [SerializeField] private bool ignoreHurtboxCheck = false;

    public bool IgnoreHurtboxCheck {
        get => ignoreHurtboxCheck;
        set => ignoreHurtboxCheck = value;
    }


    [SerializeField] private GameObject redirectTarget;

    [field: SerializeField]
    public bool RedirectActivated { get; set; } = true;
    private IHurtbox m_hurtbox;

    public IHurtbox Hurtbox => m_hurtbox;

    private void Awake() {
        m_hurtbox = redirectTarget.GetComponent<IHurtbox>();
    }
}
