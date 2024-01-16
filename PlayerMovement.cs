using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float dasdSpeed;
    public float maxYSpeed;
    public float dasdSpeedChangeFactor;
    public float Grounddrag;
    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readytojump;
    [Header("Slope Handing")]
     public float maxSlopeAngle;
     private RaycastHit slopeHit;
     private bool exitingSlope;
    [Header("Keyblinds")]
     public KeyCode jumpkey= KeyCode.Space;
     public KeyCode sprintkey = KeyCode.LeftShift;
     public KeyCode crouchkey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask WhatIsGround;
    bool grounded;
    public Transform orientation;
    float horizontal;
    float vertical;
    Vector3 moveDirection;
    Rigidbody rb;
    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        dashing,
        air
    }
    public bool dashing;
    // Start is called before the first frame update
    void Start()
    {
        rb=GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readytojump = true;
        startYScale = transform.localScale.y;
    }
     void Update()
    {
        grounded=Physics.Raycast(transform.position,Vector3.down,playerHeight *0.5f +0.2f,WhatIsGround);

        MyInput();
        SpeedControll();
        StateHandle();
        if(state == MovementState.walking || state == MovementState.sprinting || state == MovementState.crouching)
        {
            rb.drag = Grounddrag;
        }
        else
            rb.drag = 0f;
    }
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentun;
    void StateHandle()
    {
        if(dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dasdSpeed;
        }
        
        else if(Input.GetKey(crouchkey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        if(grounded && Input.GetKey(sprintkey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if(grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
             state=MovementState.air;
             
             if(desiredMoveSpeed < sprintSpeed)
             {
                desiredMoveSpeed = walkSpeed;
             }
             else
                desiredMoveSpeed = sprintSpeed;
        }     
        bool desiredMoveSpeedHasChange = desiredMoveSpeed != lastDesiredMoveSpeed;
        if(lastState == MovementState.dashing) keepMomentun = true;
        if(desiredMoveSpeedHasChange)
        {
            if(keepMomentun)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
                speedChangeFactor=dasdSpeedChangeFactor;
            }
            else
            {
                StopAllCoroutines();
                moveSpeed=desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }
    
    private float speedChangeFactor;
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            time += Time.deltaTime * boostFactor;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentun = false;
    }
   
    void FixedUpdate()
    {
        MovePlayer();
    }
    void MyInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        if(Input.GetKey(jumpkey) && readytojump && grounded)
        {
            readytojump = false;

            Jump();

            Invoke(nameof(ResetJump),jumpCooldown);
        }

        if(Input.GetKeyDown(crouchkey))
        {
            transform.localScale=new Vector3(transform.localScale.x,crouchYScale,transform.localScale.z);
            rb.AddForce(Vector3.down *5f,ForceMode.Impulse);
        }
        if(Input.GetKeyUp(crouchkey))
        {
             transform.localScale=new Vector3(transform.localScale.z,startYScale,transform.localScale.z);
        }
    }
    void MovePlayer()
    {
        if(state == MovementState.dashing) return;
        Vector3 moveDirection = orientation.forward*vertical+orientation.right*horizontal;
        if(OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection()*moveSpeed *20f,ForceMode.Force);
            if(rb.velocity.y >0)
            {
                rb.AddForce(Vector3.down * 80f,ForceMode.Force);
            }
        }
        else if(grounded)
        {
            rb.AddForce(moveDirection.normalized *moveSpeed *10f,ForceMode.Force);
        }
        else if(!grounded)
            rb.AddForce(moveDirection.normalized *moveSpeed * 10f * airMultiplier,ForceMode.Force);
    
        rb.useGravity = !OnSlope();
    }
    void SpeedControll()
    {
        if(OnSlope() && !exitingSlope)
        {
            if(rb.velocity.magnitude > moveSpeed)
               rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatvel= new Vector3(rb.velocity.x,0f,rb.velocity.z);

           if(flatvel.magnitude > moveSpeed)
           {
               Vector3 limitedVel= flatvel.normalized *moveSpeed;

               rb.velocity= new Vector3(limitedVel.x,rb.velocity.y,limitedVel.z);
           }
        }
        if(maxYSpeed !=0 && rb.velocity.y > maxYSpeed)
        {
            rb.velocity= new Vector3(rb.velocity.x,maxYSpeed,rb.velocity.z);
        }
    }
    void Jump()
    {
        exitingSlope=true;
        rb.velocity = new Vector3(rb.velocity.x,0f,rb.velocity.z);
        rb.AddForce(transform.up * jumpForce,ForceMode.Impulse);
    }
    void ResetJump()
    {
       
        readytojump = true;
         exitingSlope =false;
    }
    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight *0.5f +0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle !=0;
        }
        
        return false;
    }
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}