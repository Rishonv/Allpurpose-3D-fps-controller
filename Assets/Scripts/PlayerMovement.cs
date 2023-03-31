using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    public Transform orientation;
    public float groundDrag;
    public float slideSpeed;
    [SerializeField]private float moveSpeed;

    private float targetMoveSpeed;
    private float previousTargetMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    public float startYScale;

    [Header ("Ground Check")]
    public float playerHeight;
    public LayerMask WhatIsGround;
    [SerializeField] bool grounded;

    [Header ("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header ("Slope Handler")]
    public float maxSlopeAngle;
    //Returns information of the collider it has hit - like type, body
    private RaycastHit slopeHit;
    private bool exitingSlope;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    public MovementState state;

    private void Start(){
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;
    }

    private void Update(){
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, WhatIsGround);
        MyInput();
        SpeedControl();
        if (grounded)
            rb.drag = groundDrag;
        else rb.drag = 0;
        StateHandler();
    }

    private void FixedUpdate(){
        MovePlayer();
    }

    public enum MovementState {
        walking,
        sprinting,
        air,
        crouching,
        sliding
    }

    public bool isSliding;

    private void MyInput(){
        // Gets inputs for basic "walking" movement
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey)&& readyToJump && grounded){
            readyToJump = false;
            Jump();
            // Allows player to continually jump when pressing the jump key down
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey)){
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            // Moves player down so they aren't floating when yscale changes
            rb.AddForce(Vector3.down * 0.5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey)){
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void MovePlayer(){
        // Makes sure you move in the direction you're looking in
        moveDirection = orientation.forward * verticalInput + 
            orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope){
            rb.AddForce(GetSlopeMoveDirection(moveDirection)* moveSpeed * 20f, ForceMode.Force);
            if (rb.velocity.y > 0) rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (grounded) rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded) rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        rb.useGravity = !OnSlope();
    }

    private void StateHandler(){
        //Sliding
        if (isSliding){
            state = MovementState.sliding;
            if (OnSlope() && rb.velocity.y < 0.1f) targetMoveSpeed = slideSpeed;
            else targetMoveSpeed = sprintSpeed;
        }
        
        //Crouching
        else if (Input.GetKey(crouchKey)){
            state = MovementState.crouching;
            targetMoveSpeed = crouchSpeed;
        }
        //Sprinting
        else if (grounded && Input.GetKey(sprintKey)){
            state = MovementState.sprinting;
            targetMoveSpeed =  sprintSpeed;
        }
        //Walking
        else if (grounded){
            state = MovementState.walking;
            targetMoveSpeed = walkSpeed;
        }
        // In Air
        else{
            state = MovementState.air;
        }

        if (Mathf.Abs(previousTargetMoveSpeed - targetMoveSpeed) > 4f && moveSpeed != 0){
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        } else moveSpeed = targetMoveSpeed;
        
        previousTargetMoveSpeed = targetMoveSpeed;
    }    
    
    private IEnumerator SmoothlyLerpMoveSpeed(){
        // kind of acts like damping, smoothly moves moveSpeed to desired value
        float time = 0;
        // Mathf.Abs -> absolute value ("unsigned")
        float difference = Mathf.Abs(targetMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference){
            moveSpeed = Mathf.Lerp(startValue, targetMoveSpeed, time /difference);
            if (OnSlope()){
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                // Builds speed based on how steep the angle is
                float slopeAngleIncrease = 1 + (slopeAngle/90f);
                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }else time += Time.deltaTime * speedIncreaseMultiplier;
            yield return null;
        }
    }

    private void SpeedControl(){
        if (OnSlope() && !exitingSlope){
            if (rb.velocity.magnitude > moveSpeed){
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }else {
            // Limits the velocity to what your set speed is
            Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed){
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump(){
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump(){
        readyToJump = true;
        exitingSlope = false;
    }

    // Checks if you are on a slope
    public bool OnSlope(){
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight *0.5f +0.3f)){
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
            //Debug.Log(angle);
        }
        return false;
    }

    // Changes the forward movement of your force to an angle parallel to the slope
    public Vector3 GetSlopeMoveDirection(Vector3 direction){
        // ProjectOnPlane simply take the normal and a vector that's above the plane
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized ;
        //normalized keeps the direction of the vector but sets length to 1
    }
}