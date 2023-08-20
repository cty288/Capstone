using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitScan : IHitDetector
{
    // public BasicGun originWeapon; //TODO: change to weapon class
    public Camera cam;
    public float range;
    public LayerMask layer;

    private IHitResponder m_hitResponder;

    public IHitResponder HitResponder { get => m_hitResponder; set => m_hitResponder = value; }

    public HitScan(Camera cam, float range, LayerMask layer, IHitResponder weapon)
    {
        this.cam = cam;
        this.range = range;
        this.layer = layer;
        m_hitResponder = weapon;
    }

    public bool CheckHit()
    {
        // Debug.Log("checkhit");

        HitData hitData = null;

        Vector3 origin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(origin, cam.transform.forward, out hit, range, layer))
        {
            IHurtbox hurtbox = hit.collider.GetComponent<IHurtbox>();
            if (hurtbox != null)
            {
                hitData = new HitData().SetHitScanData(m_hitResponder, hurtbox, hit, this);
            }

            if (hitData.Validate())
            {
                // Debug.Log("validate");
                hitData.HitDetector.HitResponder?.HitResponse(hitData);
                hitData.Hurtbox.HurtResponder?.HurtResponse(hitData);
                return true;
            }
        }
        return false;
    }
}

