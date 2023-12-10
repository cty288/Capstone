using System;
using System.Collections;
using _02._Scripts.Runtime.Player.Commands;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityNavMeshAgent;
using Cinemachine;
using DG.Tweening;
using Framework;
using DG.Tweening;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using MikroFramework.Utilities;
//using MoreMountains.Feedbacks;
using Runtime.Controls;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Model;
using Runtime.Utilities;
using Runtime.Utilities.AnimatorSystem;
using Runtime.Weapons.Model.Properties;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Runtime.Player.ViewControllers
{
    [Serializable]
    public enum MovementState
    {
        walking,
        sprinting,
        sliding,
        air,
        wallrunning
    }
    
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
        [FormerlySerializedAs("vcam")] [SerializeField] 
        private CinemachineVirtualCamera defaultVirtualCamera;
        [SerializeField] 
        private CinemachineVirtualCamera secondaryVirtualCamera;
        [SerializeField] 
        private Camera fpsCamera;
        
        private float cameraPitch = 0;
        private float fpsTopClamp=90;
        private float fpsBotClamp=-90;

        [SerializeField] 
        private float defaultFOV;
        [SerializeField]
        private float runningFOV;        
        [SerializeField]
        private float slidingFOV;
        [SerializeField] 
        private float currentFOV;
        [SerializeField] 
        private float fpsFOV;

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
        
     
        [SerializeField] private float moveSpeed;
     
    
        //temporary
        private bool sprinting;

       
    
       
    
        //todo: entity data
        [SerializeField]
        private float jumpCooldown = 0.3f;
    
        //todo: entity data
        [SerializeField]
        private float airMultiplier = 0.5f;
        [SerializeField]private PhysicMaterial airMaterial;
        [SerializeField]private PhysicMaterial groundMaterial;
        [SerializeField] private Collider playerCollider;
        //temporary
        private bool readyToJump;
        
        private bool readyToDoubleJump;
        
        private float desiredMoveSpeed;
        private float lastDesiredMoveSpeed;
        
        private float speedIncreaseMultiplier = 2f;
        private float slopeIncreaseMultiplier = 3f;
        
        
        //temporary
        [Header("Ground Check")]
        private float playerHeight = 2;

        private TriggerCheck groundCheck;

        //private float airTimer = 0f;
        //public LayerMask whatIsGround;
        private bool grounded {
            get => groundCheck.Triggered;
        }

        private bool onSlope;
   
        

        
        [Header("Slope Handling")]
        [SerializeField]
        private  float maxSlopeAngle;
        [SerializeField]
        private  float minSlopeAngle;
        private RaycastHit slopeHit;
        private bool exitingSlope;
        //temporary
        [SerializeField]
        private Transform orientation;
        [SerializeField] private LayerMask slopeLayerMask;

        [Header("Sliding")]
        private float slideTimer;

        private float slideYScale = 0.25f;
        private float startYScale;
        
        private bool sliding; 
        
        [Header("Wallrunning")]
        [SerializeField]
        private LayerMask whatIsWall;
        [SerializeField]
        private float wallJumpUpForce;
        [SerializeField]
        private float wallJumpSideForce;
        //public float wallClimbSpeed;
        [SerializeField]
        private float maxWallRunTime;
        private float wallRunTimer;


        private bool wasWallRunning;
        //wallrun checks
        [SerializeField]
        private  float wallCheckDistance;
        //public float minJumpHeight;
        private RaycastHit leftWallhit;
        private RaycastHit rightWallhit;
        private bool wallLeft;
        private bool wallRight;
        
        private bool exitingWall;
        private float exitWallTime = 0.8f;
        private float exitWallTimer;
        
        private bool useGravity = false;
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

        private IGamePlayerModel gamePlayerModel;
        private IPlayerModel playerModel;

        [SerializeField]
        private MovementState state;
        
        private DPunkInputs.PlayerActions playerActions;
        private IPlayerEntity playerEntity;

        private IInventorySystem inventorySystem;
        
        //Audio
        private float walkingAudioTime = 0.5f;
        private float walkingAudioTimer = 0f;
        
        private float runningAudioTime = 0.35f;
        private float runningAudioTimer = 0f;
        
        private float wallRunAudioTime = 0.2f;
        private float wallRunAudioTimer = 0f;
        
        private AudioSource slidingAudioSource = null;

        private void Awake()
        {
            this.RegisterEvent<OnScopeUsedEvent>(SetADSFOV).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            this.RegisterEvent<OnPlayerRespawn>(OnRespawn).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            AudioSystem.Singleton.Initialize(null); //TODO: move to better spot
            
            playerActions = ClientInput.Singleton.GetPlayerActions();
            groundCheck = transform.Find("GroundCheck").GetComponent<TriggerCheck>();
        }

       

        // Start is called before the first frame update
        void Start()
        {
            defaultVirtualCamera.m_Lens.FieldOfView = defaultFOV;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            readyToJump = true;
            readyToDoubleJump = true;
            
            startYScale = model.localScale.y;
            
            gamePlayerModel = this.GetModel<IGamePlayerModel>();
            playerModel = this.GetModel<IPlayerModel>();

            playerEntity = gamePlayerModel.GetPlayer();

            inventorySystem = this.GetSystem<IInventorySystem>();

        }
        private void OnRespawn(OnPlayerRespawn obj) {
            rb.freezeRotation = true;
            gameObject.transform.rotation = Quaternion.identity;
        }
        // Update is called once per frame
        void Update()
        {
            if (gamePlayerModel.IsPlayerDead()) {
                rb.freezeRotation = false;
            }
            else
            {
                HandleCamera();
                HandleAudio();
                MyInput();
                StateHandler();
                CheckForWall();
                
                HandleAnimation();

                CheckLocation();
                onSlope = OnSlope();
            }
        }
        
        private void FixedUpdate()
        {
            //SpeedControl();
            // ground check
            //grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
            // handle drag
            if (grounded)
            {
                rb.drag = playerEntity.GetGroundDrag().RealValue;
                wasWallRunning = false;
                playerCollider.material = groundMaterial;
                
            }
            else {
                rb.drag = playerEntity.GetAirDrag().RealValue;
                playerCollider.material = airMaterial;
            }
                
            MovePlayer();
            
        }

        private void HandleAudio()
        {
            if (state == MovementState.walking && rb.velocity.magnitude >= 0.1f)
            {
                walkingAudioTimer += Time.deltaTime;
                if (walkingAudioTimer >= walkingAudioTime)
                {
                    AudioSystem.Singleton.Play2DSound("FootSteps");
                    walkingAudioTimer = 0f;
                }
            }
            else if (state == MovementState.sprinting && rb.velocity.magnitude >= 0.1f)
            {
                runningAudioTimer += Time.deltaTime;
                if (runningAudioTimer >= runningAudioTime)
                {
                    AudioSystem.Singleton.Play2DSound("FootSteps");
                    runningAudioTimer = 0f;
                }
            }

            if (state == MovementState.wallrunning)
            {
                wallRunAudioTimer += Time.deltaTime;
                if (wallRunAudioTimer >= wallRunAudioTime)
                {
                    AudioSystem.Singleton.Play2DSound("FootSteps");
                    wallRunAudioTimer = 0f;
                }
            }
            
            if (state == MovementState.sliding)
            {
                if (slidingAudioSource == null)
                {
                    slidingAudioSource = AudioSystem.Singleton.Play2DSound("slide_3", 1f, false);
                }
            }
            else
            {
                if (slidingAudioSource != null) {
                    slidingAudioSource = null;
                    AudioSystem.Singleton.StopSound("slide_3");
                }
            }
            
            // JUMP
        }
        
        private void StateHandler()
        {
            float weaponWeight = 1f;
            IResourceEntity heldEntity = inventorySystem.GetCurrentlySelectedEntity();
            if (heldEntity != null && heldEntity.GetResourceCategory() == ResourceCategory.Weapon)
            {
                weaponWeight = heldEntity.GetProperty<IWeight>().RealValue;
            }
            
            // Mode - Wallrunning
            if (wallrunning)
            {
                state = MovementState.wallrunning;
                playerEntity.SetMovementState(state);

                 desiredMoveSpeed = playerEntity.GetSprintSpeed().RealValue * weaponWeight;
            }
            // Mode - Sliding
            else if (sliding)
            {
                state = MovementState.sliding;
                playerEntity.SetMovementState(state);

                if (onSlope && rb.velocity.y < 0.1f)
                    desiredMoveSpeed = playerEntity.GetSlideSpeed().RealValue * weaponWeight;

                else
                    desiredMoveSpeed = playerEntity.GetSprintSpeed().RealValue * weaponWeight;
            }

            // Mode - Sprinting
            else if(grounded && sprinting)
            {
                state = MovementState.sprinting;
                playerEntity.SetMovementState(state);

                desiredMoveSpeed = playerEntity.GetSprintSpeed().RealValue;
            }

            // Mode - Walking
            else if (grounded)
            {
                state = MovementState.walking;
                playerEntity.SetMovementState(state);
                if (playerEntity.IsScopedIn())
                {
                    desiredMoveSpeed = playerEntity.GetWalkSpeed().RealValue * 0.5f * weaponWeight;
                }
                else
                {
                    desiredMoveSpeed = playerEntity.GetWalkSpeed().RealValue * weaponWeight;
                }
            }

            // Mode - Air
            else
            {
                state = MovementState.air;
                playerEntity.SetMovementState(state);
                desiredMoveSpeed = playerEntity.GetAirSpeed().RealValue;
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
                if (currentFOV != defaultFOV && !playerEntity.IsScopedIn())
                {
                    SetFOV(defaultFOV);
                }
                
                if(fpsCamera.fieldOfView != fpsFOV)
                    fpsCamera.DOFieldOfView(fpsFOV, 0.1f);

                
                if (horizontalInput == 0 &&verticalInput == 0)
                    ChangeBobVars(idleBob);
                else
                    ChangeBobVars(walkBob);
            }
            else if (state == MovementState.sliding)
            {
                if (currentFOV != slidingFOV && !playerEntity.IsScopedIn())
                {
                    SetFOV(slidingFOV);
                }
                
                if(fpsCamera.fieldOfView != fpsFOV - 10)
                    fpsCamera.DOFieldOfView(fpsFOV - 5, 0.1f);

                ChangeBobVars(0,0);
            }
            else if (state == MovementState.sprinting||(state == MovementState.air&& sprinting))
            {
                if (currentFOV != runningFOV && !playerEntity.IsScopedIn())
                {
                    SetFOV(runningFOV);
                }
                
                if(fpsCamera.fieldOfView != fpsFOV - 10)
                    fpsCamera.DOFieldOfView(fpsFOV - 3, 0.1f);

                ChangeBobVars(sprintBob);
            }
            else
            {
                if (currentFOV != defaultFOV && !playerEntity.IsScopedIn())
                {
                    SetFOV(defaultFOV);
                }
                
                if(fpsCamera.fieldOfView != fpsFOV)
                    fpsCamera.DOFieldOfView(fpsFOV, 0.1f);
                
                ChangeBobVars(0,0);
            }
        }

        private void HandleAnimation()
        {
            if (state == MovementState.walking && rb.velocity.magnitude >= 0.1f)
            {
                this.SendCommand<PlayerAnimationCommand>(
                    PlayerAnimationCommand.Allocate("Moving", AnimationEventType.Bool, 1));
                this.SendCommand<PlayerAnimationCommand>(
                    PlayerAnimationCommand.Allocate("MoveSpeed", AnimationEventType.Float, 1.5f));
            }
            else if (state == MovementState.sprinting && rb.velocity.magnitude >= 0.1f)
            {
                this.SendCommand<PlayerAnimationCommand>(
                    PlayerAnimationCommand.Allocate("Moving", AnimationEventType.Bool, 1));
                this.SendCommand<PlayerAnimationCommand>(
                    PlayerAnimationCommand.Allocate("MoveSpeed", AnimationEventType.Float, 2f));
                
                
                
                // MMChannelData newData = new MMChannelData(MMChannelModes.Int, 0, null);
                // MMCameraZoomEvent.Trigger(MMCameraZoomModes.Set, 0, 0.1f, 0.1f,  newData, false, true, false, true, null);
            }
            else
            {
                this.SendCommand<PlayerAnimationCommand>(
                    PlayerAnimationCommand.Allocate("Moving", AnimationEventType.Bool, 0));
            }
            
            if (state == MovementState.sliding && rb.velocity.magnitude >= 0.1f)
            {
                this.SendCommand<PlayerAnimationCommand>(
                    PlayerAnimationCommand.Allocate("Sliding", AnimationEventType.Bool, 1));
            } else
            {
                this.SendCommand<PlayerAnimationCommand>(
                    PlayerAnimationCommand.Allocate("Sliding", AnimationEventType.Bool, 0));
            }
        }

        private void SetFOV(float fov)
        {
            if (defaultVirtualCamera.Priority == 10)
            {
                secondaryVirtualCamera.m_Lens.FieldOfView = fov;
                secondaryVirtualCamera.Priority = 10;
                defaultVirtualCamera.Priority = 1;
            }
            else
            {
                defaultVirtualCamera.m_Lens.FieldOfView = fov;
                defaultVirtualCamera.Priority = 10;
                secondaryVirtualCamera.Priority = 1;
            }
            
            currentFOV = fov;
        }

        private void SetADSFOV(OnScopeUsedEvent e)
        {
            IResourceEntity heldEntity = inventorySystem.GetCurrentlySelectedEntity();
            if (heldEntity != null && heldEntity.GetResourceCategory() == ResourceCategory.Weapon)
            {
                SetFOV(e.isScopedIn ? heldEntity.GetProperty<IAdsFOV>().RealValue : currentFOV);
            }
        }

        public void ChangeBobVars(HeadBobData data)
        {
            defaultVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = data.FrequencyGain;
            defaultVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = data.AmplitudeGain;
            
            secondaryVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = data.FrequencyGain;
            secondaryVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = data.AmplitudeGain;
        }
        public void ChangeBobVars(float frequencyGain,float amplitudeGain)
        {
            defaultVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = frequencyGain;
            defaultVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = amplitudeGain;
            
            secondaryVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = frequencyGain;
            secondaryVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = amplitudeGain;
        }
        public void DoCamTilt(float zTilt)
        {
            cameraTrans.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
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
                wasWallRunning = false;

                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
            }
            if (playerActions.Jump.WasPressedThisFrame() && readyToDoubleJump &&!grounded&&!wallrunning)
            {
                readyToDoubleJump = false;
                wasWallRunning = false;

                Jump();
                
            }
            
            if (playerActions.SprintHold.IsPressed()) {
                sprinting = true;
                //Debug.Log("Sprinting");
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
                rb.AddForce(moveDirection*10f,ForceMode.Impulse);
                slideTimer = playerEntity.GetMaxSlideTime().RealValue;

            }

            if (playerActions.Slide.WasReleasedThisFrame() && sliding)
            {
                sliding = false;

                model.localScale = new Vector3(model.localScale.x, startYScale, model.localScale.z);
                
                //camera
                DoCamTilt(0f);
            }
            
            if((wallLeft || wallRight) && verticalInput > 0 && !grounded && !exitingWall)
            {
                
                if (playerActions.SprintHold.IsPressed())
                {
                    if (!wasWallRunning&&!wallrunning)
                    {
                        StartWallRun();
                    }
                    if (playerActions.SprintHold.WasPressedThisFrame())
                    {
                        if (!wallrunning)
                            StartWallRun();
                    }
                    
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
                else
                {
                    if (wallrunning)
                        StopWallRun();
                }

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
            
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude < moveSpeed) {
 
                
                if (sliding)
                    SlidingMovement();
                else if (wallrunning)
                    WallRunningMovement();
                else if (onSlope && !exitingSlope) {
                    rb.AddForce(GetSlopeMoveDirection(moveDirection) * playerEntity.GetAccelerationForce().RealValue, ForceMode.Force);

                    if (rb.velocity.y < 0)
                        rb.AddForce(-slopeHit.normal * 80f, ForceMode.Force);
                    else
                        rb.AddForce(Vector3.down * 80f, ForceMode.Force);
                }
                // on ground
                else if (grounded)
                {
                    
                    rb.AddForce(moveDirection.normalized * playerEntity.GetAccelerationForce().RealValue, ForceMode.Force);
                    /*//stop player if no inputs
                    if (moveDirection == Vector3.zero)
                    {
                        rb.velocity = new Vector3(0, rb.velocity.y, 0);
                    }*/
                }
                // in air
                else if (!grounded)
                {
                    rb.AddForce(moveDirection.normalized * playerEntity.GetAccelerationForce().RealValue * airMultiplier, ForceMode.Force);
                }
            }
           // Debug.Log("Grounded ");
            if (!grounded && !onSlope && !wallrunning){
              
                rb.AddForce((-transform.up)* playerEntity.GetAdditionalGravity().RealValue,ForceMode.Force);
            }
            
            // turn gravity off while on slope
            //if(!wallrunning)rb.useGravity = !onSlope;

            
            if (moveDirection == Vector3.zero && lastMoveDirection != Vector3.zero) {
                if (grounded || (onSlope && !exitingSlope)) {
                    rb.velocity = new Vector3(0, rb.velocity.y, 0);;
                }
            }
            
            lastMoveDirection = moveDirection;

        }



        private void Jump()
        {
            exitingSlope = true;
            // reset y velocity
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * playerEntity.GetJumpForce().RealValue, ForceMode.Impulse);
        }
        private void ResetJump()
        {

            readyToJump = true;
            
            exitingSlope = false;
        }
        
        
        public bool OnSlope()
        {
            if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f, slopeLayerMask))
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                    // Debug.Log(angle);
                return angle < maxSlopeAngle && angle >minSlopeAngle;
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

                if (onSlope)
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
            if(!onSlope || rb.velocity.y > -0.1f)
            {
                rb.AddForce(inputDirection.normalized * playerEntity.GetSlideForce().RealValue  *0.8f, ForceMode.Force);
                rb.AddForce(-GetSlopeMoveDirection(inputDirection) * playerEntity.GetSlideForce().RealValue*0.25f, ForceMode.Force);
                slideTimer -= Time.deltaTime;
                
            }
            // sliding down a slope
            else
            {
                rb.AddForce(GetSlopeMoveDirection(inputDirection) * playerEntity.GetSlideForce().RealValue , ForceMode.Force);
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

            if (wallLeft) DoCamTilt(-25f);
            if (wallRight) DoCamTilt(25f);
        }
        
        private void StopWallRun()
        {
            wallrunning = false;
            wasWallRunning = true;

            // reset camera effects

            DoCamTilt(0f);
        }
        
        private void WallJump()
        {
            // enter exiting wall state
            exitingWall = true;
            exitWallTimer = exitWallTime;
            readyToDoubleJump = true;

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
            rb.AddForce(wallForward * playerEntity.GetWallRunForce().RealValue, ForceMode.Force);
            
            if(wallRunTimer<=0.5f)
                rb.velocity = new Vector3(rb.velocity.x, -5f, rb.velocity.z);

            // upwards/downwards force
            /*if (upwardsRunning)
                rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
            if (downwardsRunning)
                */

            // push to wall force
            if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
                rb.AddForce(-wallNormal * 100, ForceMode.Force);

            // weaken gravity
            if (useGravity)
                rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        }

        private void CheckLocation()
        {
            NavMeshHit hit;
            NavMesh.SamplePosition(transform.position, out hit, 100f, NavMesh.AllAreas);
            if (hit.hit)
            {
                print($"player current area: {hit.mask}");
                playerModel.CurrentSubAreaMask.Value = hit.mask;
            }
        }
        
        public Rigidbody GetRigidBody()
        {
            return rb;
        }
    }
}

