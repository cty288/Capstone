using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour, IHitDetector
{
    [SerializeField] private BoxCollider m_collider;
    [SerializeField] private LayerMask layerMask;


    private float thickness = 0.025f;
    private IHitResponder m_hitResponder;

    public IHitResponder HitResponder { get => m_hitResponder; set => m_hitResponder = value; }

    public bool CheckHit()
    {
        // Debug.Log("checkhit");
        Vector3 scaledSize = new Vector3(
            m_collider.size.x * transform.lossyScale.x,
            m_collider.size.y * transform.lossyScale.y,
            m_collider.size.z * transform.lossyScale.z
        );
        float distance = scaledSize.y - thickness;
        Vector3 direction = transform.up;
        Vector3 center = transform.TransformPoint(m_collider.center);
        Vector3 start = center + direction * (distance * 0.5f);
        Vector3 halfExtents = new Vector3(scaledSize.x, thickness, scaledSize.z) / 2;
        Quaternion orientation = transform.rotation;

        HitData hitData = null;
        IHurtbox hurtbox = null;
        RaycastHit[] hits = Physics.BoxCastAll(start, halfExtents, direction, orientation, distance, layerMask);
        foreach (RaycastHit hit in hits)
        {
            // Debug.Log("raycasthit: " + hit.point);
            hurtbox = hit.collider.GetComponent<IHurtbox>();
            if (hurtbox != null)
            {
                hitData = new HitData().SetHitBoxData(m_hitResponder, hurtbox, hit, this, center);
            }

            if (hitData.Validate())
            {
                // Debug.Log("validate");
                hitData.HitDetector.HitResponder?.HitResponse(hitData);
                hitData.Hurtbox.HurtResponder?.HurtResponse(hitData);
            }
        }

        if (hits.Length > 0)
            return true;
        else
            return false;
    }
}

