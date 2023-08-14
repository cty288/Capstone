using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IHitResponder, IHurtResponder
{
    //todo: entity data
    public int maxHealth = 100;
    //todo: entity data
    public int curHealth = 100;

    [SerializeField] private int m_damage = 10;
    public int Damage => m_damage;

    private List<GameObject> hitObjects = new List<GameObject>();


    public bool CheckHit(HitData data)
    {
        throw new System.NotImplementedException();
    }

    public bool CheckHurt(HitData data)
    {
        throw new System.NotImplementedException();
    }

    public void HitResponse(HitData data)
    {
        throw new System.NotImplementedException();
    }

    public void HurtResponse(HitData data)
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(int damage)
    {
        curHealth -= damage;
        Debug.Log("Player takes " + damage + " damage.");
    }
}
