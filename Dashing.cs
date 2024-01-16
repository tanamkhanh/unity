using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dashing : MonoBehaviour
{
    [Header("References")]
    public Transform Orientation;
    public Transform playercam;
    private Rigidbody rb;
    private PlayerMovement pm;
    [Header("CameraEffects")]
    public ThirdPlayerCam cam;
    public float dashFov;
    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float maxDashYSpeed;
    public float dashDuration;
    [Header("Setting")]
    public bool useCameraForward=true;
    public bool allowAllDirection=true;
    public bool disableGravity=false;
    public bool resetVel=true;
    [Header("Cooldown")]
    public float dashcd;
    private float dashCdTimer;
    [Header("Input")]
    public KeyCode dashKey=KeyCode.E;
 
    void Start()
    {
        rb=GetComponent<Rigidbody>();
        pm=GetComponent<PlayerMovement>(); 
    }

    void Update()
    { 
        if(Input.GetKey(dashKey))
            Dash();
        if(dashCdTimer > 0)
        {
            dashCdTimer -= Time.deltaTime;
        }
    }
    private void Dash()
    {
        if(dashCdTimer > 0) return;
        else dashCdTimer = dashcd;

        pm.dashing = true;
        pm.maxYSpeed=maxDashYSpeed;
        cam.DoFov(dashFov);

        Transform forwardT;
        if(useCameraForward)
            forwardT=playercam;
        else
            forwardT=Orientation;
        Vector3 direction = GetDirection(forwardT);
        Vector3 forceToApply=direction * dashForce + Orientation.up * dashUpwardForce;
        if(disableGravity)
        rb.useGravity=false;

       delayForceToApply = forceToApply;
        Invoke(nameof(DelayDashForce),0.025f);

        Invoke(nameof(ResetDash),dashDuration);
    }
    private Vector3 delayForceToApply;
    private void DelayDashForce()
    {
        if(resetVel)
        {
            rb.velocity=Vector3.zero;
        }
         rb.AddForce(delayForceToApply,ForceMode.Impulse);
    }
    private void ResetDash()
    {
        pm.dashing = false;
        pm.maxYSpeed=0f;

        cam.DoFov(85f);

        if(disableGravity)
           rb.useGravity=true;
    }
    private Vector3 GetDirection(Transform forwardT)
    {
        float horizontal=Input.GetAxisRaw("Horizontal");
        float vertical=Input.GetAxisRaw("Vertical");

        Vector3 direction=new Vector3();
        if(allowAllDirection)
        direction = forwardT.forward*vertical+forwardT.right*horizontal;
        else
        direction=forwardT.forward;
        if(vertical == 0 && horizontal == 0)
        direction=forwardT.forward;

        return direction.normalized;
    }
}
