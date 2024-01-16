using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ThirdPlayerCam : MonoBehaviour
{
  [Header("References")]
  public Transform orientation;
  public Transform player;
  public Transform PlayerObj;
  public Rigidbody rb;
  public float rotationSpeed;
  public float turnSmoothTime =0.1f;
  float turnSmoothVelocity;
  public Transform cambatLookAt;
  public GameObject ThirdPersonCam;
  public GameObject combatCam;
  public GameObject TopDownCam;
  public CameraStyle currentStyle;
  public enum CameraStyle
  {
    Basic,
    Combat,
    TopDown,
  }
   
   void Start()
   {
    Cursor.lockState = CursorLockMode.Locked;
   }
   void Update()
   {
    if(Input.GetKeyDown(KeyCode.Alpha1)) SwitchCamStyle(CameraStyle.Basic);
    if(Input.GetKeyDown(KeyCode.Alpha2)) SwitchCamStyle(CameraStyle.Combat);
    if(Input.GetKeyDown(KeyCode.Alpha3)) SwitchCamStyle(CameraStyle.TopDown);

    Vector3 viewDir = player.position -new Vector3(transform.position.x,player.position.y,transform.position.z);
    orientation.forward = viewDir.normalized;

    if(currentStyle == CameraStyle.Basic || currentStyle == CameraStyle.TopDown)
    {
     float horizontal=Input.GetAxis("Horizontal");
     float vertical=Input.GetAxis("Vertical");

     Vector3 inputDir=orientation.forward  * vertical + orientation.right * horizontal ;

     if(inputDir !=Vector3.zero)
     {
         PlayerObj.forward = Vector3.Slerp(PlayerObj.forward,inputDir.normalized,Time.deltaTime * rotationSpeed);
     }
    }
    else if(currentStyle == CameraStyle.Combat)
    {
        Vector3 dirToCambatLookAt = cambatLookAt.position -new Vector3(transform.position.x,player.position.y,transform.position.z);
    orientation.forward = dirToCambatLookAt.normalized;

    PlayerObj.forward= dirToCambatLookAt.normalized;
    }
   }
    void SwitchCamStyle (CameraStyle newStyle)
   {
    combatCam.SetActive(false);
    ThirdPersonCam.SetActive(false);
    TopDownCam.SetActive(false);

    if(currentStyle == CameraStyle.Basic) ThirdPersonCam.SetActive(true);
    if(currentStyle == CameraStyle.Combat) combatCam.SetActive(true);
    if(currentStyle == CameraStyle.TopDown) TopDownCam.SetActive(true);

    currentStyle = newStyle;
   }
   public void DoFov(float endValue)
   {
    GetComponent<Camera>().DOFieldOfView(endValue,0.25f);
   }
}
