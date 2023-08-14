using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitData
{
    public int Damage;
    public Vector3 HitPoint;
    public Vector3 HitNormal;
    public IHurtbox Hurtbox;
    public IHitDetector HitDetector;

    // public HitData() { }

    public HitData SetHitScanData(IHitResponder hitResponder, IHurtbox hurtbox, RaycastHit hit, IHitDetector hitDetector)
    {
        Damage = hitResponder == null ? 0 : Mathf.FloorToInt(hitResponder.Damage * hurtbox.DamageMultipler);
        HitPoint = hit.point;
        HitNormal = hit.normal;
        Hurtbox = hurtbox;
        HitDetector = hitDetector;
        return this;
    }

    public HitData SetHitBoxData(IHitResponder hitResponder, IHurtbox hurtbox, RaycastHit hit, IHitDetector hitDetector, Vector3 hitBoxCenter)
    {
        Damage = hitResponder == null ? 0 : Mathf.FloorToInt(hitResponder.Damage * hurtbox.DamageMultipler);
        HitPoint = hit.point == Vector3.zero ? hitBoxCenter : hit.point;
        HitNormal = hit.normal;
        Hurtbox = hurtbox;
        HitDetector = hitDetector;
        return this;
    }

    public bool Validate()
    {
        if (Hurtbox != null)
            if (Hurtbox.CheckHit(this))
                if (Hurtbox.HurtResponder == null || Hurtbox.HurtResponder.CheckHurt(this))
                    if (HitDetector.HitResponder == null || HitDetector.HitResponder.CheckHit(this))
                        return true;
        return false;
    }
}

//hitresponder waits for hitdetector to find collision with hurtbox. A hit generates hitData, which propagates back to both responders.
public interface IHitResponder
{
    int Damage { get; }
    public bool CheckHit(HitData data);
    public void HitResponse(HitData data);
}

public interface IHitDetector
{
    public IHitResponder HitResponder { get; set; }
    public bool CheckHit();
}

public interface IHurtResponder
{
    public bool CheckHurt(HitData data);
    public void HurtResponse(HitData data);
}
public interface IHurtbox
{
    public bool Active { get; }
    public GameObject Owner { get; } //TODO: change to entity
    public Transform Transform { get; }
    public IHurtResponder HurtResponder { get; set; }
    public float DamageMultipler { get; set; }
    public bool CheckHit(HitData data);

}