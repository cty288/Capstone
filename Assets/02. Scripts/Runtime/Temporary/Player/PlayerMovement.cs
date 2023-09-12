using System;
using Mikrocosmos;
using Mikrocosmos.Controls;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Runtime.Temporary.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField]
        float mouseSensitivity = 3.5f;
        [SerializeField]
        float controllerSensitivity = 10f;
        

        [SerializeField]
        Transform cameraTrans;
    
        float cameraPitch = 0;
        public float fpsTopClamp=90;
        public float fpsBotClamp=-90;

    
        //TODO: add acceleration to entity data
        //TODO: add maxSpeed to entity data
    
        //temporary data
        [Header("Movement")] 
        public float accelForce;
        private float moveSpeed;
        
        public float walkSpeed;
        public float sprintSpeed;
        public float slideSpeed;
    
        //temporary
        private bool sprinting;

        //todo: entity data
        public float groundDrag;
    
        //jump todo: entity data
        public float jumpForce;
    
        //todo: entity data
        public float jumpCooldown;
    
        //todo: entity data
        public float airMultiplier;
    
        //temporary
        bool readyToJump;
        
        private bool readyToDoubleJump;
        
        private float desiredMoveSpeed;
        private float lastDesiredMoveSpeed;

        public float speedIncreaseMultiplier;
        public float slopeIncreaseMultiplier;
        
        
        //temporary
        [Header("Ground Check")]
        public float playerHeight;
        public LayerMask whatIsGround;
        bool grounded;

        public float additionalGravity;

        
        [Header("Slope Handling")]
        public float maxSlopeAngle;
        private RaycastHit slopeHit;
        private bool exitingSlope;
        //temporary
        public Transform orientation;

        //temporary
        float horizontalInput;
        float verticalInput;

        Vector3 moveDirection;

        public Rigidbody rb;

        public bool sliding;
        public MovementState state;
        public enum MovementState
        {
            walking,
            sprinting,
            sliding,
            air
        }
        
        private DPunkInputs.PlayerActions playerActions;

        private void Awake() {
            playerActions = ClientInput.Singleton.GetPlayerActions();
        }

        // Start is called before the first frame update
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            readyToJump = true;
            readyToDoubleJump = true;

        }

        // Update is called once per frame
        void Update()
        {
            
            HandleCamera();

            // ground check
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

            MyInput();
            SpeedControl();
            StateHandler();

            // handle drag
            if (grounded)
                rb.drag = groundDrag;
            else
                rb.drag = 0;
        }
        private void FixedUpdate()
        {      
            MovePlayer();
            if (Input.GetKeyDown(KeyCode.K)) {
                GameObject obj = ControlInfoFactory.Singleton.GetBindingKeyGameObject(ClientInput.Singleton.FindActionInPlayerActionMap("Sprint"),
                    out BindingInfo info, out string actionName);
                //Debug.Log("Action Name: " + actionName);
            }
            Debug.Log(OnSlope());
        }
        private void StateHandler()
        {
            // Mode - Sliding
            if (sliding)
            {
                state = MovementState.sliding;

                if (OnSlope() && rb.velocity.y < 0.1f)
                    desiredMoveSpeed = slideSpeed;

                else
                    desiredMoveSpeed = sprintSpeed;
            }
                

            // Mode - Sprinting
            else if(grounded && sprinting)
            {
                state = MovementState.sprinting;
                desiredMoveSpeed = sprintSpeed;
            }

            // Mode - Walking
            else if (grounded)
            {
                state = MovementState.walking;
                desiredMoveSpeed = walkSpeed;
            }

            // Mode - Air
            else
            {
                state = MovementState.air;
            }

            // check if desiredMoveSpeed has changed drastically
            if(Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed;
            }

            lastDesiredMoveSpeed = desiredMoveSpeed;
        }
        private void HandleCamera()
        {
            //Camera;
            
            //Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            Vector2 mouseDelta = playerActions.Look.ReadValue<Vector2>();
            //Debug.Log(mouseDelta);
            float sensitivity = ClientInput.Singleton.PlayerInput.currentControlScheme == "Gamepad" ? controllerSensitivity : mouseSensitivity;
            cameraPitch -= mouseDelta.y * sensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, fpsBotClamp, fpsTopClamp);
            cameraTrans.localEulerAngles = Vector3.right * cameraPitch;
            transform.Rotate(Vector3.up * mouseDelta.x * sensitivity);
        }
        private void MyInput()
        {
            horizontalInput = playerActions.Move.ReadValue<Vector2>().x;
            verticalInput = playerActions.Move.ReadValue<Vector2>().y;

            // when to jump
            if (playerActions.Jump.WasPressedThisFrame() && readyToJump && grounded)
            {
                readyToJump = false;
                readyToDoubleJump = true;

                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
            }
            if (playerActions.Jump.WasPressedThisFrame() && readyToDoubleJump &&!grounded)
            {
                readyToDoubleJump = false;

                Jump();
                
            }
            if (playerActions.Sprint.IsPressed())
            {
                sprinting = true;
                Debug.Log("Sprinting");
            }
            else
            {
                sprinting = false;
            }
        }

        private void MovePlayer()
        {
            // calculate movement direction
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            float spd = accelForce;
            // on slope
            if (OnSlope() && !exitingSlope)
            {
                rb.AddForce(GetSlopeMoveDirection(moveDirection) * spd, ForceMode.Force);

                if (rb.velocity.y > 0)
                    rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
            // on ground
            else if (grounded)
            {
                rb.AddForce(moveDirection.normalized * spd, ForceMode.Force);
                //stop player if no inputs
                if (moveDirection == Vector3.zero)
                {
                    rb.velocity = new Vector3(0, rb.velocity.y, 0);
                }
            }
                
            // in air
            else if (!grounded)
            {
                rb.AddForce(moveDirection.normalized * spd * airMultiplier, ForceMode.Force);
                if(!OnSlope())
                    rb.AddForce((-transform.up)*additionalGravity,ForceMode.Force);
            }
            
            // turn gravity off while on slope
            rb.useGravity = !OnSlope();

        }

        private void SpeedControl()
        {
            // limiting speed on slope
            if (OnSlope() && !exitingSlope)
            {
                if (rb.velocity.magnitude > moveSpeed)
                    rb.velocity = rb.velocity.normalized * moveSpeed;
            }

            // limiting speed on ground or in air
            else
            {
                Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

                // limit velocity if needed
                if (flatVel.magnitude > moveSpeed)
                {
                    Vector3 limitedVel = flatVel.normalized * moveSpeed;
                    rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
                }
            }
        }

        private void Jump()
        {
            exitingSlope = true;
            // reset y velocity
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        private void ResetJump()
        {

            readyToJump = true;
            
            exitingSlope = false;
        }
    
        private Vector3 GetDirection()
        {
            float horizontalInput = playerActions.Move.ReadValue<Vector2>().x;
            float verticalInput = playerActions.Move.ReadValue<Vector2>().y;

            Vector3 direction = new Vector3();

            direction = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (verticalInput == 0 && horizontalInput == 0)
                direction = orientation.forward;

            return direction.normalized;
        }
        
        public bool OnSlope()
        {
            if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                return angle < maxSlopeAngle && angle != 0;
            }

            return false;
        }
        private IEnumerator SmoothlyLerpMoveSpeed()
        {
            // smoothly lerp movementSpeed to desired value
            float time = 0;
            float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
            float startValue = moveSpeed;

            while (time < difference)
            {
                moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

                if (OnSlope())
                {
                    float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                    float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                    time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
                }
                else
                    time += Time.deltaTime * speedIncreaseMultiplier;

                yield return null;
            }

            moveSpeed = desiredMoveSpeed;
        }
        
        public Vector3 GetSlopeMoveDirection(Vector3 direction)
        {
            return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
        }
    }
}

