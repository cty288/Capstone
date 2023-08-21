using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour, IHurtbox
{
    [SerializeField] private bool m_active = true;
    [SerializeField] private GameObject m_owner = null;

    private IHurtResponder m_hurtResponder;
    [SerializeField] private float m_damageMultiplier = 1f;

    public bool Active => m_active;

    public GameObject Owner => m_owner;

    public Transform Transform => transform;

    public IHurtResponder HurtResponder { get => m_hurtResponder; set => m_hurtResponder = value; }
    public float DamageMultipler { get => m_damageMultiplier; set => m_damageMultiplier = value; }

    private void Awake()
    {
        if (m_owner == null)
            m_owner = transform.root.gameObject;
    }

    public bool CheckHit(HitData data)
    {
        if (m_hurtResponder == null)
            Debug.Log("hurt responder is null");

        return true;
    }
}
