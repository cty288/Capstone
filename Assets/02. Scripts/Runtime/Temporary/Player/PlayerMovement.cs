using System;
using System.Collections;
using Framework;
using Mikrocosmos;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Runtime.Controls;
using Cinemachine;
using DG.Tweening;
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
        [SerializeField]
        Transform camHolder;
        
        [SerializeField] 
        CinemachineVirtualCamera vcam;
    
        float cameraPitch = 0;
        [SerializeField] 
        float fpsTopClamp=90;
        [SerializeField] 
        float fpsBotClamp=-90;

        [SerializeField] 
        private float defaultFOV;


        [Header("Headbob")] 
        [SerializeField] 
        private HeadBobData idleBob;
        [SerializeField] 
        private HeadBobData walkBob;
        [SerializeField] 
        private HeadBobData sprintBob;
        [Serializable]
        public struct HeadBobData
        {
            public float AmplitudeGain;
            public float FrequencyGain;

            public HeadBobData(float amplitudeGain,float frequencyGain)
            {
                this.AmplitudeGain = amplitudeGain;
                this.FrequencyGain = frequencyGain;
            }
        }
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
        [SerializeField] private LayerMask slopeLayerMask;

        [Header("Sliding")]
        public float maxSlideTime;
        public float slideForce;
        private float slideTimer;

        public float slideYScale;
        private float startYScale;
        
        public bool sliding;
        
        [Header("Wallrunning")]
        public LayerMask whatIsWall;
        public float wallRunForce;
        public float wallJumpUpForce;
        public float wallJumpSideForce;
        //public float wallClimbSpeed;
        public float maxWallRunTime;
        private float wallRunTimer;
        
        //wallrun checks
        public float wallCheckDistance;
        //public float minJumpHeight;
        private RaycastHit leftWallhit;
        private RaycastHit rightWallhit;
        private bool wallLeft;
        private bool wallRight;
        
        private bool exitingWall;
        public float exitWallTime;
        private float exitWallTimer;
        
        public bool useGravity;
        public float gravityCounterForce;

        private bool wallrunning;
        //temporary
        float horizontalInput;
        float verticalInput;

        Vector3 moveDirection;

        public Rigidbody rb;

        public Transform model;

        public MovementState state;
        public enum MovementState
        {
            walking,
            sprinting,
            sliding,
            air,
            wallrunning
        }
        
        private DPunkInputs.PlayerActions playerActions;

        private void Awake() {
            playerActions = ClientInput.Singleton.GetPlayerActions();
        }

        // Start is called before the first frame update
        void Start()
        {
            vcam.m_Lens.FieldOfView = defaultFOV;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            readyToJump = true;
            readyToDoubleJump = true;
            
            startYScale = model.localScale.y;

        }

        // Update is called once per frame
        void Update()
        {
            //Camera;
            
            HandleCamera();

            // ground check
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

            MyInput();
            SpeedControl();
            StateHandler();
            CheckForWall();

            // handle drag
            if (grounded)
                rb.drag = groundDrag;
            else
                rb.drag = 0;

            if (Input.GetKeyDown(KeyCode.F5)) {
                ((MainGame) MainGame.Interface).SaveGame();
            }
        }
        private bool sprintTapPressed = false;
        private float sprintTapCheckTimer = 0.4f;
        
        private void FixedUpdate()
        {      
            MovePlayer();
            if (Input.GetKeyDown(KeyCode.K)) {
                GameObject obj = ControlInfoFactory.Singleton.GetBindingKeyGameObject(ClientInput.Singleton.FindActionInPlayerActionMap("Sprint"),
                    out BindingInfo info, out string actionName);
                //Debug.Log("Action Name: " + actionName);
            }
            // Debug.Log(OnSlope());
        }
        
        private void StateHandler()
        {
            // Mode - Wallrunning
            if (wallrunning)
            {
                state = MovementState.wallrunning;

                 desiredMoveSpeed = sprintSpeed;
            }
            // Mode - Sliding
            else if (sliding)
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
            camHolder.localEulerAngles = Vector3.right * cameraPitch;
            transform.Rotate(Vector3.up * mouseDelta.x * sensitivity);

            
            if (state == MovementState.walking)
            {
                if (horizontalInput == 0 &&verticalInput == 0)
                    ChangeBobVars(idleBob);
                else
                    ChangeBobVars(walkBob);
            }
            else if (state == MovementState.sprinting)
            {
                ChangeBobVars(sprintBob);
            }
            else
            {
                ChangeBobVars(0,0);
            }

            
        }

        public void ChangeBobVars(HeadBobData data)
        {
            vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = data.FrequencyGain;
            vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = data.AmplitudeGain;
        }
        public void ChangeBobVars(float frequencyGain,float amplitudeGain)
        {
            vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = frequencyGain;
            vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = amplitudeGain;
        }
        public void DoCamTilt(float zTilt)
        {
            cameraTrans.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
        }
        
        //smooth FOV transition
        IEnumerator ChangeFOV(CinemachineVirtualCamera cam, float endFOV, float duration)
        {
            float startFOV = cam.m_Lens.FieldOfView;
            float time = 0;
            while(time < duration)
            {
                cam.m_Lens.FieldOfView = Mathf.Lerp(startFOV, endFOV, time / duration);
                yield return null;
                time += Time.deltaTime;
            }
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
            ;
            if (playerActions.SprintTap.WasPerformedThisFrame()) {
                Debug.Log("Sprint Tap");
                sprintTapPressed = true;
                sprintTapCheckTimer = 0.4f;
            }
            
            
            
            if (playerActions.SprintHold.IsPressed() || sprintTapPressed) {
                sprinting = true;
                Debug.Log("Sprinting");
            }
            else
            {
                sprinting = false;
            }

            if (playerActions.Slide.WasPressedThisFrame() &&(horizontalInput != 0 || verticalInput != 0))
            {
                sliding = true;
                model.localScale = new Vector3(model.localScale.x, slideYScale, model.localScale.z);
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

                slideTimer = maxSlideTime;
                
            }

            if (playerActions.Slide.WasReleasedThisFrame() && sliding)
            {
                sliding = false;

                model.localScale = new Vector3(model.localScale.x, startYScale, model.localScale.z);
                
                //camera
                DoCamTilt(0f);
            }
            
            if((wallLeft || wallRight) && verticalInput > 0 && !grounded && !exitingWall&&playerActions.SprintHold.IsPressed())
            {
                if (!wallrunning)
                    StartWallRun();

                // wallrun timer
                if (wallRunTimer > 0)
                    wallRunTimer -= Time.deltaTime;

                if(wallRunTimer <= 0 && wallrunning)
                {
                    exitingWall = true;
                    exitWallTimer = exitWallTime;
                }

                // wall jump
                if (playerActions.Jump.WasPressedThisFrame() ) WallJump();
            }
            else if (exitingWall)
            {
                if (wallrunning)
                    StopWallRun();

                if (exitWallTimer > 0)
                    exitWallTimer -= Time.deltaTime;

                if (exitWallTimer <= 0)
                    exitingWall = false;
            }
            else
            {
                if (wallrunning)
                    StopWallRun();
            }
        }

        private void MovePlayer()
        {
            // calculate movement direction
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            float spd = accelForce;
            if (SlopeTooBig())
            {
                rb.AddForce(slopeHit.normal * 1000, ForceMode.Force);
            }
            // on slope
            else if (OnSlope() && !exitingSlope)
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
            if(!wallrunning)rb.useGravity = !OnSlope();

            if (sliding)
                SlidingMovement();
            if (wallrunning)
                WallRunningMovement();
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
            
            //stop player if no inputs
            if (moveDirection == Vector3.zero)
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
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
            if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f, slopeLayerMask))
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                return angle < maxSlopeAngle && angle != 0;
            }

            return false;
        }

        public bool SlopeTooBig()
        {
            if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f, slopeLayerMask))
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                return angle >= maxSlopeAngle && angle != 0;
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
        
        private void SlidingMovement()
        {
            Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (horizontalInput > 0)
            {
                DoCamTilt(-5f);
            }
            else if (horizontalInput < 0)
            {
                DoCamTilt(5f);
            }
            else
            {
                DoCamTilt(0f);
            }
            // sliding normal
            if(!OnSlope() || rb.velocity.y > -0.1f)
            {
                rb.AddForce(inputDirection.normalized * slideForce*0.5f, ForceMode.Force);

                slideTimer -= Time.deltaTime;
                
            }

            // sliding down a slope
            else
            {
                rb.AddForce(GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
            }

            if (slideTimer <= 0)
            {
                sliding = false;

                model.localScale = new Vector3(model.localScale.x, startYScale, model.localScale.z);
                
                //camera
                DoCamTilt(0f);
            }
            
        }
        
        private void CheckForWall()
        {
            wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
            wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
        }
        
        private void StartWallRun()
        {
            wallrunning = true;

            wallRunTimer = maxWallRunTime;

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // apply camera effects

            if (wallLeft) DoCamTilt(-5f);
            if (wallRight) DoCamTilt(5f);
        }
        
        private void StopWallRun()
        {
            wallrunning = false;

            // reset camera effects

            DoCamTilt(0f);
        }
        
        private void WallJump()
        {
            // enter exiting wall state
            exitingWall = true;
            exitWallTimer = exitWallTime;

            Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

            Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

            // reset y velocity and add force
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(forceToApply, ForceMode.Impulse);
        }
        
        private void WallRunningMovement()
        {
            rb.useGravity = useGravity;

            Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

            Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

            if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
                wallForward = -wallForward;

            // forward force
            rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

            // upwards/downwards force
            /*if (upwardsRunning)
                rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
            if (downwardsRunning)
                rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);*/

            // push to wall force
            if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
                rb.AddForce(-wallNormal * 100, ForceMode.Force);

            // weaken gravity
            if (useGravity)
                rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        }
    }
}

