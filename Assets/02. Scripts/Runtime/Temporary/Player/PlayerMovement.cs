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
using MikroFramework.Architecture;
using MikroFramework.Utilities;
using Runtime.Player;

namespace Runtime.Temporary.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : AbstractMikroController<MainGame>
    {
        [Header("Camera")]
        [SerializeField]
        private float mouseSensitivity = 3.5f;
        [SerializeField]
        private float controllerSensitivity = 10f;
        

        [SerializeField]
        private Transform cameraTrans;
        [SerializeField]
        private Transform camHolder;
        [SerializeField] 
        private CinemachineVirtualCamera vcam;
    
        private float cameraPitch = 0;
        private float fpsTopClamp=90;
        private float fpsBotClamp=-90;

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
        
        [SerializeField]
        private float accelForce;
        private float moveSpeed;
        [SerializeField]
        private float walkSpeed;
        [SerializeField]
        private float sprintSpeed;
        [SerializeField]
        private float slideSpeed;
    
        //temporary
        private bool sprinting;

        //todo: entity data
        [SerializeField]
        private float groundDrag;
    
        //jump todo: entity data
        [SerializeField]
        private float jumpForce;
    
        //todo: entity data
        private float jumpCooldown = 0.3f;
    
        //todo: entity data
        private float airMultiplier = 0.3f;
    
        //temporary
        private bool readyToJump;
        
        private bool readyToDoubleJump;
        
        private float desiredMoveSpeed;
        private float lastDesiredMoveSpeed;

        [SerializeField]
        private float speedIncreaseMultiplier = 1.5f;
        [SerializeField]
        private float slopeIncreaseMultiplier = 2.5f;
        
        
        //temporary
        [Header("Ground Check")]
        private float playerHeight = 2;

        private TriggerCheck groundCheck;
        //public LayerMask whatIsGround;
        private bool grounded {
            get => groundCheck.Triggered;
        }

        [SerializeField]
        private  float additionalGravity;

        
        [Header("Slope Handling")]
        [SerializeField]
        private  float maxSlopeAngle;
        private RaycastHit slopeHit;
        private bool exitingSlope;
        //temporary
        [SerializeField]
        private Transform orientation;
        [SerializeField] private LayerMask slopeLayerMask;

        [Header("Sliding")]
        [SerializeField]
        private  float maxSlideTime;
        [SerializeField]
        private  float slideForce;
        private float slideTimer;

        private float slideYScale = 0.25f;
        private float startYScale;
        
        private bool sliding;
        
        [Header("Wallrunning")]
        [SerializeField]
        private LayerMask whatIsWall;
        [SerializeField]
        private float wallRunForce;
        [SerializeField]
        private float wallJumpUpForce;
        [SerializeField]
        private float wallJumpSideForce;
        //public float wallClimbSpeed;
        [SerializeField]
        private float maxWallRunTime;
        private float wallRunTimer;
        
        //wallrun checks
        [SerializeField]
        private  float wallCheckDistance;
        //public float minJumpHeight;
        private RaycastHit leftWallhit;
        private RaycastHit rightWallhit;
        private bool wallLeft;
        private bool wallRight;
        
        private bool exitingWall;
        private float exitWallTime = 0.3f;
        private float exitWallTimer;
        
        private bool useGravity = true;
        [SerializeField]
        private float gravityCounterForce;

        private bool wallrunning;
        //temporary
        float horizontalInput;
        float verticalInput;

        private Vector3 moveDirection;
        private Vector3 lastMoveDirection;
        private Rigidbody rb;
        [SerializeField]
        private Transform model;

        [SerializeField]
        private MovementState state;
        [Serializable]
        private enum MovementState
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
            groundCheck = transform.Find("GroundCheck").GetComponent<TriggerCheck>();
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
            



            IGamePlayerModel playerModel = this.GetModel<IGamePlayerModel>();
            if (playerModel.IsPlayerDead())
            {
                rb.freezeRotation = false;
            }
            else
            {
                HandleCamera();

            

                MyInput();
            
                StateHandler();
                CheckForWall();
            }
            
            
            
            if (Input.GetKeyDown(KeyCode.F5)) {
                ((MainGame) MainGame.Interface).SaveGame();
            }
        }

        
        private void FixedUpdate()
        {
            //SpeedControl();
            // ground check
            //grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
            // handle drag
            if (grounded)
                rb.drag = groundDrag;
            else
                rb.drag = 0;
            MovePlayer();
            
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
            
            
            if (playerActions.SprintHold.IsPressed()) {
                sprinting = true;
                //Debug.Log("Sprinting");
            }
            if (playerActions.SprintHold.WasReleasedThisFrame())
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
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude < moveSpeed) {
                if (SlopeTooBig())
                {
                    rb.AddForce(slopeHit.normal * 1000, ForceMode.Force);
                }
                // on slope
                else if (OnSlope() && !exitingSlope) {
                    rb.AddForce(GetSlopeMoveDirection(moveDirection) * spd, ForceMode.Force);

                    //if (rb.velocity.y > 0)
                    //  rb.AddForce(Vector3.down * 80f, ForceMode.Force);
                }
                // on ground
                else if (grounded)
                {
                    rb.AddForce(moveDirection.normalized * spd, ForceMode.Force);
                    /*//stop player if no inputs
                    if (moveDirection == Vector3.zero)
                    {
                        rb.velocity = new Vector3(0, rb.velocity.y, 0);
                    }*/
                }
                // in air
                else if (!grounded)
                {
                    rb.AddForce(moveDirection.normalized * spd * airMultiplier, ForceMode.Force);
                }
                
               
                
                if (sliding)
                    SlidingMovement();
                if (wallrunning)
                    WallRunningMovement();
            }

            if (!grounded) {
                if(!OnSlope()&&!wallrunning)
                    rb.AddForce((-transform.up)*additionalGravity,ForceMode.Force);
            }
            

            
            
            // turn gravity off while on slope
            if(!wallrunning)rb.useGravity = !OnSlope();
            if (flatVel.magnitude < moveSpeed) {
                if (sliding)
                    SlidingMovement();
                if (wallrunning)
                    WallRunningMovement();
            }

            
            if (moveDirection == Vector3.zero && lastMoveDirection != Vector3.zero) {
                if (grounded || (OnSlope() && !exitingSlope)) {
                    rb.velocity -= flatVel;
                }
            }
            
            lastMoveDirection = moveDirection;

        }

        /*private void SpeedControl()
        {
            // limiting speed on slope
            if (OnSlope() && !exitingSlope)
            {
                //if (rb.velocity.magnitude > moveSpeed)
                  //  rb.velocity = rb.velocity.normalized * moveSpeed;
                //stop player if no inputs
                if (moveDirection == Vector3.zero)
                {
                    
                   // rb.velocity = new Vector3(0, rb.velocity.y, 0);
                }
            }

            // limiting speed on ground or in air
            else
            {
                Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

                // limit velocity if needed
                if (flatVel.magnitude > moveSpeed) {
                    
                    //Vector3 limitedVel = flatVel.normalized * moveSpeed;
                    //rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
                    
                }
            }
            

        }*/

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
                    // Debug.Log(angle);
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
                rb.AddForce(inputDirection.normalized * slideForce*0.3f, ForceMode.Force);
                rb.AddForce(-GetSlopeMoveDirection(inputDirection) * slideForce*0.25f, ForceMode.Force);
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

