using System;
using Runtime.Controls;
using UnityEngine;

namespace Runtime.Weapons
{
    public class WeaponSway : MonoBehaviour
    {
        [SerializeField] public Transform weaponTransform;

        [Header("Sway Properties")]
        [SerializeField]private float swayAmount = 0.01f;
        [SerializeField] public float maxSwayAmount = 0.1f;
        [SerializeField] public float swaySmooth = 9f;
        [SerializeField] public AnimationCurve swayCurve;

        [Range(0f, 1f)]
        [SerializeField] public float swaySmoothCounteraction = 1f;

        [Header("Rotation")]
        [SerializeField] public float rotationSwayMultiplier = 1f;

        [Header("Position")]
        [SerializeField] public float positionSwayMultiplier = -1f;
    

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector2 sway;
        
        private DPunkInputs.PlayerActions playerActions;

        private void Reset()
        {
            Keyframe[] ks = new Keyframe[] { new Keyframe(0, 0, 0, 2), new Keyframe(1, 1) };
            swayCurve = new AnimationCurve(ks);
        }

        private void Start()
        {
            playerActions = ClientInput.Singleton.GetPlayerActions();
            
            if (!weaponTransform)
                weaponTransform = transform;
            initialPosition = weaponTransform.localPosition;
            initialRotation = weaponTransform.localRotation;
        }

        private void Update()
        {
            float mouseX = playerActions.Look.ReadValue<Vector2>().x * swayAmount;
            float mouseY = playerActions.Look.ReadValue<Vector2>().y * swayAmount;

            sway = Vector2.MoveTowards(sway, Vector2.zero, swayCurve.Evaluate(Time.deltaTime * swaySmoothCounteraction * sway.magnitude * swaySmooth));
            sway = Vector2.ClampMagnitude(new Vector2(mouseX, mouseY) + sway, maxSwayAmount);

            weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, new Vector3(sway.x, sway.y, 0) * positionSwayMultiplier + initialPosition, swayCurve.Evaluate(Time.deltaTime * swaySmooth));
            weaponTransform.localRotation = Quaternion.Slerp(transform.localRotation, initialRotation * Quaternion.Euler(Mathf.Rad2Deg * rotationSwayMultiplier * new Vector3(-sway.y, sway.x, 0)), swayCurve.Evaluate(Time.deltaTime * swaySmooth));
        }
    }
}