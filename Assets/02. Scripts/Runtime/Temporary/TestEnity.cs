using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnity : MonoBehaviour
{
    public string m_name = "_unnamed_";
    public int maxHealth = 100;
    public int curHealth = 100;

    public void TakeDamage(int damage)
    {
        curHealth -= damage;
        Debug.Log(m_name + " takes " + damage + " damage.");
    }

    public void HealFull()
    {
        curHealth = maxHealth;
        Debug.Log(m_name + " heals to full.");
    }
}
