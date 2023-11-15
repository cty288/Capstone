using System;
using Framework;
using MikroFramework.Architecture;
using Runtime.Controls;
using Runtime.Player;
using Runtime.Player.ViewControllers;
using Runtime.Utilities;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace Runtime.Weapons
{
    public class WeaponSway : AbstractMikroController<MainGame>
    {
        [SerializeField] public Transform weaponTransform;

        [Header("Sway Properties")]
        [SerializeField]private float swayAmount = 0.01f;
        [SerializeField] public float maxSwayAmountHipfire = 0.1f;
        [SerializeField] public float maxSwayAmountADS = 0.01f;
        [SerializeField] public float swaySmooth = 9f;
        [SerializeField] public AnimationCurve swayCurve;

        [Range(0f, 1f)]
        [SerializeField] public float swaySmoothCounteraction = 1f;

        [Header("Rotation")]
        [SerializeField] public float rotationSwayMultiplier = 1f;

        [Header("Position")]
        [SerializeField] public float positionSwayMultiplier = -1f;
    

        [SerializeField]
        private Vector3 initialPosition;

        public Vector3 InitialPosition {
            get => initialPosition;
            set => initialPosition = value;
        }
        
        
        private Quaternion initialRotation;
        private Vector2 sway;
        
        private DPunkInputs.PlayerActions playerActions;
        private Rigidbody playerRb;
        private IGamePlayerModel playerModel;

        
        public bool isADS = false;

        private void Awake()
        {
            this.RegisterEvent<OnScopeUsedEvent>(SetADSStatus).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            playerModel = this.GetModel<IGamePlayerModel>();
        }

        private void Reset()
        {
            Keyframe[] ks = new Keyframe[] { new Keyframe(0, 0, 0, 2), new Keyframe(1, 1) };
            swayCurve = new AnimationCurve(ks);
        }

        private void Start()
        {
            playerActions = ClientInput.Singleton.GetPlayerActions();
            playerRb = GetComponentInParent<PlayerMovement>().GetComponent<Rigidbody>();
            
            if (!weaponTransform)
                weaponTransform = transform;
            initialPosition = weaponTransform.localPosition;
            initialRotation = weaponTransform.localRotation;
        }

        private void Update()
        {
            if (!playerModel.IsPlayerDead())
            {
                float maxSwayAmount = isADS ? maxSwayAmountADS : maxSwayAmountHipfire;
                float mouseX = playerActions.Look.ReadValue<Vector2>().x * swayAmount;
                float mouseY = playerActions.Look.ReadValue<Vector2>().y * swayAmount;
            
                Vector3 localPlayerVelocity = transform.InverseTransformDirection(playerRb.velocity);
                float forwardMovement = localPlayerVelocity.x * swayAmount;
                float sidewaysMovement = localPlayerVelocity.z * swayAmount;
                float verticalMovement = localPlayerVelocity.y * swayAmount;
            
                sway = Vector2.MoveTowards(sway, Vector2.zero, swayCurve.Evaluate(Time.deltaTime * swaySmoothCounteraction * sway.magnitude * swaySmooth));
                sway = Vector2.ClampMagnitude(new Vector2(mouseX + forwardMovement, mouseY + sidewaysMovement + 2 * verticalMovement) + sway, maxSwayAmount);

                weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, new Vector3(sway.x, sway.y, 0) * positionSwayMultiplier + initialPosition, swayCurve.Evaluate(Time.deltaTime * swaySmooth));
                weaponTransform.localRotation = Quaternion.Slerp(transform.localRotation, initialRotation * Quaternion.Euler(Mathf.Rad2Deg * rotationSwayMultiplier * new Vector3(-sway.y, sway.x, 0)), swayCurve.Evaluate(Time.deltaTime * swaySmooth));
            }
        }
        
        private void SetADSStatus(OnScopeUsedEvent e)
        {
            this.isADS = e.isScopedIn;
        }
    }
}