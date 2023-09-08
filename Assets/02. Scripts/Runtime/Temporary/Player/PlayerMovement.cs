using System;
using Mikrocosmos;
using Mikrocosmos.Controls;
using UnityEngine;

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
        public float moveSpeed;

        //TODO: add maxSpeed to entity data
        public float sprintmultiplier;
    
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
    

        //temporary
        [Header("Ground Check")]
        public float playerHeight;
        public LayerMask whatIsGround;
        bool grounded;

        //temporary
        public Transform orientation;

        //temporary
        float horizontalInput;
        float verticalInput;

        Vector3 moveDirection;

        public Rigidbody rb;


        private DPunkInputs.PlayerActions playerActions;

        private void Awake() {
            playerActions = ClientInput.Singleton.GetPlayerActions();
        }

        // Start is called before the first frame update
        void Start()
        {
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = false;

            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            readyToJump = true;

        }

        // Update is called once per frame
        void Update()
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


            // ground check
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

            MyInput();
            SpeedControl();

            // handle drag
            if (grounded)
                rb.drag = groundDrag;
            else
                rb.drag = 0;
        }
        private void FixedUpdate()
        {      
            MovePlayer();
        }

        private void MyInput()
        {
            horizontalInput = playerActions.Move.ReadValue<Vector2>().x;
            verticalInput = playerActions.Move.ReadValue<Vector2>().y;

            // when to jump
            if (playerActions.Jump.WasPressedThisFrame() && readyToJump && grounded)
            {
                readyToJump = false;

                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
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

            float spd = moveSpeed;
            if (sprinting)
            {
                spd = moveSpeed * sprintmultiplier;
            }
            // on ground
            if (grounded)
                rb.AddForce(moveDirection.normalized * spd * 10f, ForceMode.Force);

            // in air
            else if (!grounded)
                rb.AddForce(moveDirection.normalized * spd * 10f * airMultiplier, ForceMode.Force);
        }

        private void SpeedControl()
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            float curSpeed = moveSpeed;
            if (sprinting)
            {
                curSpeed = moveSpeed * sprintmultiplier;
            }
            // limit velocity if needed
            if (flatVel.magnitude > curSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

        private void Jump()
        {
            // reset y velocity
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        private void ResetJump()
        {

            readyToJump = true;
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
    }
}

